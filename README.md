📱 ChatApp - Web 即時通訊平台

🚀 一個類似 LINE 的 Web 即時聊天應用，支援即時訊息傳遞、群組聊天、圖片分享、WebSocket 連線等功能。

✨ 功能特色
✅ 即時聊天 - 使用 WebSocket 技術，支援單人與群組聊天
✅ 訊息通知 - 當收到新訊息時即時通知
✅ 圖片/檔案傳輸 - 支援圖片與檔案上傳與預覽
✅ 使用者狀態 - 顯示上線/離線狀態
✅ 訊息已讀/未讀 - 追蹤訊息是否已讀
✅ 登入/註冊 - 透過 JWT 身份驗證使用者
✅ 管理後台 - 可管理用戶、群組與聊天紀錄

🛠 技術架構
前端 (Frontend)
Vue.js 3 + Vite
Tailwind CSS
Axios (API 請求)
WebSocket (即時通訊)
後端 (Backend)
.NET Core 8 (C#)
ASP.NET Web API
SignalR (即時 WebSocket 連線)
Entity Framework Core + SQL Server
Dapper (高效 SQL 查詢)
Redis (快取與即時通知)
🎯 快速開始
1️⃣ 環境準備
請確保你的環境已安裝以下工具：

Node.js 16+
.NET 8 SDK
SQL Server
Redis
2️⃣ 克隆專案
bash
複製
編輯
git clone https://github.com/你的帳號/chatapp-web.git
cd chatapp-web
3️⃣ 安裝與啟動前端
bash
複製
編輯
cd frontend
npm install
npm run dev
4️⃣ 安裝與啟動後端
bash
複製
編輯
cd backend
dotnet restore
dotnet run
5️⃣ 設定環境變數 (.env)
請參考 .env.example 並填寫你的資料庫連線資訊。

📷 預覽截圖
登入畫面	聊天室	群組聊天
📜 授權條款
本專案基於 MIT 授權，詳細內容請參考 LICENSE 文件。

🤝 貢獻指南
歡迎提交 Issue 或 PR，一起讓 ChatApp 更加完善！

Fork 本專案
創建新分支 (git checkout -b feature-xxx)
提交更改 (git commit -m "新增 xxx 功能")
推送分支 (git push origin feature-xxx)
發送 PR
📬 聯絡方式
如果有任何問題，請透過以下方式聯絡我：
📧 Email: example@email.com
💬 Discord: mydiscord#1234
