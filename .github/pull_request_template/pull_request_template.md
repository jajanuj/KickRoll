## 摘要
> 說明這個 PR 做了什麼、為何需要。

- 關聯 Issue：Closes #<id>

## 變更類型
- [ ] 修 Bug
- [ ] 新功能
- [ ] 重構 / 技術債
- [ ] 文件/設定

## 實作重點
-

## 測試計畫
- 指令：`dotnet restore && dotnet build KickRoll.sln -c Release && dotnet test KickRoll.sln -c Release`
- 重要案例：
  -

## 螢幕截圖 / 錄影（UI 變更必填）
<貼上圖片或 GIF>

## 風險與回滾方案
- 潛在風險：
- 回滾方法：

## 安全性 / 相容性檢查
- [ ] 不包含硬編碼機密，設定改由環境變數/Secrets 提供
- [ ] CORS 設定合理（僅允許必要來源）
- [ ] API 變更有版本控管或為向下相容

## 清單（由人或 Copilot Agent 勾選）
- [ ] `dotnet format` 無差異（或已修正）
- [ ] 單元測試涵蓋新/改功能；CI 綠燈
- [ ] Swagger（API）已更新（如有端點變更）
- [ ] MAUI（App）變更已附資源/字串、可存取性檢查
- [ ] PR 描述包含 Summary / Risk / Test Plan / Rollback

<!-- 給 Copilot Agent 的提示：
- 請遵守 /.github/copilot-instructions.md 與 /.github/instructions/*.instructions.md
- 若測試缺失，先補測試再提交
- PR 範圍過大時，請分成多個較小 PR
-->
