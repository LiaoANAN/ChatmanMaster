<h1>📱 Chatman - 即時通訊APP(PWA WEB)</h1>
<p>
    <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
</p>
<p>🚀 一個類似 LINE 的 Web 即時聊天應用，支援即時訊息傳遞、群組聊天、圖片分享、WebSocket 連線等功能。</p>

<hr>

<h2>✨ 功能特色</h2>
<ul>
    <li>✅ <b>即時聊天</b> - 使用 WebSocket 技術，支援單人與群組聊天</li>
    <li>✅ <b>訊息通知</b> - 當收到新訊息時即時通知</li>
    <li>✅ <b>圖片/檔案傳輸</b> - 支援圖片與檔案上傳與預覽</li>
    <li>✅ <b>使用者狀態</b> - 顯示上線/離線狀態</li>
    <li>✅ <b>訊息已讀/未讀</b> - 追蹤訊息是否已讀</li>
    <li>✅ <b>登入/註冊</b> - 透過 JWT 身份驗證使用者</li>
    <li>✅ <b>管理後台</b> - 可管理用戶、群組與聊天紀錄</li>
</ul>

<hr>

<h2>🛠 技術架構</h2>

<h3>前端 (Frontend)</h3>
<ul>
    <li>Vue.js 3 + Vite</li>
    <li>Tailwind CSS</li>
    <li>Axios (API 請求)</li>
    <li>WebSocket (即時通訊)</li>
</ul>

<h3>後端 (Backend)</h3>
<ul>
    <li>.NET Core 8 (C#)</li>
    <li>ASP.NET Web API</li>
    <li>SignalR (即時 WebSocket 連線)</li>
    <li>Entity Framework Core + SQL Server</li>
    <li>Dapper (高效 SQL 查詢)</li>
    <li>Redis (快取與即時通知)</li>
</ul>

<hr>

<h2>🎯 快速開始</h2>

<h3>1️⃣ 環境準備</h3>
<p>請確保你的環境已安裝以下工具：</p>
<ul>
    <li>Node.js 16+</li>
    <li>.NET 8 SDK</li>
    <li>SQL Server</li>
    <li>Redis</li>
</ul>

<h3>2️⃣ 克隆專案</h3>
<pre><code>git clone https://github.com/你的帳號/chatapp-web.git
cd chatapp-web
</code></pre>

<h3>3️⃣ 安裝與啟動前端</h3>
<pre><code>cd frontend
npm install
npm run dev
</code></pre>

<h3>4️⃣ 安裝與啟動後端</h3>
<pre><code>cd backend
dotnet restore
dotnet run
</code></pre>

<h3>5️⃣ 設定環境變數 (.env)</h3>
<p>請參考 <code>.env.example</code> 並填寫你的資料庫連線資訊。</p>

<hr>

<h2>📷 預覽截圖</h2>
<table>
    <tr>
        <th>登入畫面</th>
        <th>聊天室</th>
        <th>群組聊天</th>
    </tr>
    <tr>
        <td><img src="https://via.placeholder.com/200" alt="Login"></td>
        <td><img src="https://via.placeholder.com/200" alt="Chat"></td>
        <td><img src="https://via.placeholder.com/200" alt="GroupChat"></td>
    </tr>
</table>

<hr>

<h2>📜 授權條款</h2>
<p>本專案基於 MIT 授權，詳細內容請參考 <a href="LICENSE">LICENSE</a> 文件。</p>

<hr>

<h2>🤝 貢獻指南</h2>
<p>歡迎提交 Issue 或 PR，一起讓 ChatApp 更加完善！</p>
<ol>
    <li>Fork 本專案</li>
    <li>創建新分支 (<code>git checkout -b feature-xxx</code>)</li>
    <li>提交更改 (<code>git commit -m "新增 xxx 功能"</code>)</li>
    <li>推送分支 (<code>git push origin feature-xxx</code>)</li>
    <li>發送 PR</li>
</ol>

<hr>

<h2>📬 聯絡方式</h2>
<p>如果有任何問題，請透過以下方式聯絡我：</p>
<ul>
    <li>📧 Email: example@email.com</li>
    <li>💬 Discord: mydiscord#1234</li>
</ul>
