# 部署到 Azure（App Service + Azure SQL 免費方案）

把原本 GCP（Cloud Run + Cloud SQL）的架構搬到 Azure：
- **App → Azure App Service（F1 免費層）**
- **資料庫 → Azure SQL Database（Free offer）**
- **日常更新 → push 到 `main` 由 GitHub Actions 自動部署**（見最後一節）

> 已經部署完成的目前線上網址：https://mypocket-web-app.azurewebsites.net
> 以下步驟 1–3 是「從零重建」時的紀錄；日常更新只需 `git push`。

> 你有 Azure for Students：US$100 額度（到 2027-06-22）、免綁信用卡。
> 就算偶爾超出免費額度，也是從這 $100 扣，碰不到你自己的錢。

App 程式碼**不用改**。Program.cs 從設定讀 `ConnectionStrings:MyPocketDBConnection`，
`DbInitializer.Initialize()` 用 `EnsureCreatedAsync()`，第一次啟動會自動建表 + 種子資料，
所以**不需要匯入 `MyPocketDB.bacpac`**，建一個空資料庫即可。

---

## 前置
- 已登入 Azure for Students 的帳號
- 本機已裝 .NET 8 SDK 與 Azure CLI（`az`）；macOS 用 `brew install azure-cli`

---

## 步驟 1：建立 Azure SQL Database（Free offer）

1. Azure Portal → **Create a resource** → **SQL Database**
2. 基本設定：
   - Resource group：新建 `mypocket-rg`
   - Database name：`MyPocketDB`
   - Server：新建一個 server（記下 **server 名稱**、**管理員帳號**、**密碼**）
3. **Compute + storage** → Configure → Service tier 選 **General Purpose → Serverless**，
   勾選 **「Use free offer」(Free offer)**。
4. **Networking** → 開啟 **Allow Azure services and resources to access this server**。
5. 建好後，到 SQL Database → **Connection strings → ADO.NET**，複製連線字串，
   把裡面的 `{your_password}` 換成你設定的密碼，備用。

---

## 步驟 2：建立 App Service 並首次部署（az CLI）

```bash
az login
# 建立 App Service（Linux, F1 免費層）並部署目前程式碼
cd <專案根目錄>
dotnet publish MyPocket.Web/MyPocket.Web.csproj -c Release -o ./publish
cd publish && zip -r ../publish.zip . > /dev/null && cd ..
az webapp create --resource-group mypocket-rg --plan <plan名> --name mypocket-web-app --runtime "DOTNETCORE:8.0"
az webapp deployment source config-zip --resource-group mypocket-rg --name mypocket-web-app --src publish.zip
```

> 之後的更新不必再跑這些，已改由 GitHub Actions 自動處理（見最後一節）。

---

## 步驟 3：設定連線字串（唯一要設的東西）

到 Azure Portal → 你的 App Service → **Settings → Environment variables**
（舊介面是 Configuration → Application settings）→ 新增：

| Name | Value |
|------|-------|
| `ConnectionStrings__MyPocketDBConnection` | `Server=tcp:<server>.database.windows.net,1433;Database=MyPocketDB;User ID=<帳號>;Password=<密碼>;Encrypt=True;TrustServerCertificate=False;` |

> ⚠️ 是**雙底線** `__`，.NET 才會對應到 `ConnectionStrings:MyPocketDBConnection`。

存檔 → App Service 重啟 → 第一次啟動自動建表 + 種子資料。

---

## 完成
App Service → Overview → **Default domain** 點開，網站就活了。

### 之後要更新版本 → push 即自動部署（GitHub Actions）

已設定 CI/CD：**只要 push 到 `main`，GitHub Actions 會自動 build 並部署到 Azure**。
你不需要手動跑任何指令。

```bash
git add -A
git commit -m "你的修改說明"
git push        # 推上 main 後，幾分鐘內網站自動更新
```

- workflow 設定檔：[.github/workflows/deploy.yml](.github/workflows/deploy.yml)
- 登入方式：OIDC 聯合憑證（GitHub Secrets 只存 `AZURE_CLIENT_ID` /
  `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID`，**無長期密碼**）
- 手動觸發：GitHub repo → **Actions** 分頁 → 「Build and deploy to Azure」→ Run workflow
- 看部署狀態 / log：同樣在 **Actions** 分頁

<details>
<summary>備援：手動部署指令（CI 壞掉時才需要）</summary>

```bash
dotnet publish MyPocket.Web/MyPocket.Web.csproj -c Release -o ./publish
cd publish && zip -r ../publish.zip . > /dev/null && cd ..
az webapp deployment source config-zip --resource-group mypocket-rg --name mypocket-web-app --src publish.zip
```
</details>

### 預設管理員帳號
種子資料於 `DbInitializer.cs` 建立：
- Email：`admin@example.com`
- 密碼：`12345678`
- ⚠️ 公開 repo 可見，建議登入後立即修改。

### F1 免費層限制（Demo 夠用）
- 每天 60 分鐘 CPU、會休眠（第一次開比較慢）、不能綁自訂網域 SSL。
- 若要更穩，可改 B1（約 $13/月），你的 $100 額度可撐約 7 個月。

### 費用提醒
- 建議到 Cost Management 設一個 Budget 警示（例如超過 US$5 寄信通知）。
