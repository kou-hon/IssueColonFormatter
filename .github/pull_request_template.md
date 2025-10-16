












以上が、プルリクエスト本文のテンプレート
<!-- I want to review in Japanese. -->
<!-- for GitHub Copilot review rule -->

<details>
<summary>for GitHub Copilot review rule</summary>

### お願い

- Pull Request Overview、Reviewed Changes、コメントは全て必ず日本語で書いてください

### 1. レビュー対象と範囲

- 次の観点でコード品質の改善提案を行ってください
  - ロジックの正確性
  - セキュリティ上の問題
  - パフォーマンスの最適化
  - データ競合の可能性
  - 一貫性の維持
  - エラーハンドリング
  - 保守性の向上
  - モジュール性の確保
  - コードの複雑性軽減
  - 最適化の可能性
  - ベストプラクティス(DRY, SOLID, KISS)の適用
- 重大な問題（例：テスト失敗、脆弱性、O(n²) 以上のボトルネック）に絞ってレビューしてください
- 以下についてはコメントしないでください
  - 軽微なコードスタイルの問題
  - コメント/ドキュメントの不足
  - `db/**`, `**/*.lock`
  - 自動生成されたコード（l10n, freezedなど）

</details>
<!-- for GitHub Copilot review rule -->
<!-- I want to review in Japanese. -->