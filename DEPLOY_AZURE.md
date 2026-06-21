# 部署到 Azure（Visual Studio 一鍵 Publish + Azure SQL 免費方案）

把原本 GCP（Cloud Run + Cloud SQL）的架構搬到 Azure：
- **App → Azure App Service（F1 免費層）**，用 Visual Studio Enterprise 直接 Publish，不碰 Docker
- **資料庫 → Azure SQL Database（Free offer）**

> 你有 Azure for Students：US$100 額度（到 2027-06-22）、免綁信用卡。
> 就算偶爾超出免費額度，也是從這 $100 扣，碰不到你自己的錢。

App 程式碼**不用改**。Program.cs 從設定讀 `ConnectionStrings:MyPocketDBConnection`，
`DbInitializer.Initialize()` 用 `EnsureCreatedAsync()`，第一次啟動會自動建表 + 種子資料，
所以**不需要匯入 `MyPocketDB.bacpac`**，建一個空資料庫即可。

---

## 前置
- 已登入 Azure for Students 的帳號
- Visual Studio Enterprise 2022（你的學生方案已含，免費）

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

## 步驟 2：用 Visual Studio Publish 到 App Service

1. Visual Studio 開啟 `MyPocketSystem.sln`
2. 右鍵 **MyPocket.Web** 專案 → **Publish**
3. Target 選 **Azure** → **Azure App Service (Linux)** → Next
4. 登入你的 Azure 帳號 → **Create new** App Service：
   - Resource group：用同一個 `mypocket-rg`
   - **Hosting Plan → 新建 → Size 選 Free (F1)**
   - 建立並選取
5. 按 **Finish** → **Publish**，VS 會自動 build 並上傳，完成後會打開網站網址。

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

### 之後要更新版本
在 VS 重新按 Publish 即可。

### F1 免費層限制（Demo 夠用）
- 每天 60 分鐘 CPU、會休眠（第一次開比較慢）、不能綁自訂網域 SSL。
- 若要更穩，可改 B1（約 $13/月），你的 $100 額度可撐約 7 個月。

### 費用提醒
- 建議到 Cost Management 設一個 Budget 警示（例如超過 US$5 寄信通知）。
