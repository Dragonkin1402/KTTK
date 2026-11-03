# Danh sách biểu đồ cần dùng cho báo cáo/thuyết trình NLP

Tài liệu này tập hợp các biểu đồ quan trọng từ Lab 1, 3, 4 và 5, kèm đường dẫn artefact đã có và trạng thái W&B. Hình ảnh nhúng dưới đây là ảnh được tạo/lưu trong workspace; chỉ gọi là **biểu đồ W&B online** khi run đã được đồng bộ lên W&B cloud.

## Ưu tiên 1 — Confusion Matrix

### Lab 4 — GRU, IMDB

**Nên trình bày:** confusion matrix của GRU được chọn bằng validation macro-F1. Biểu đồ thể hiện rõ false positive (negative → positive) và false negative (positive → negative).

![Confusion matrix GRU Lab 4](./Lab4/outputs/runs/gru_baseline_online_20261015/figures/confusion_matrix.png)

- Bản lưu trong workspace: [confusion_matrix.png](./Lab4/outputs/runs/gru_baseline_online_20261015/figures/confusion_matrix.png)
- W&B run: [GRU — gru_baseline_online_20261015](https://wandb.ai/nmanhcuong1402-dnu/csc4007-lab4-lstm-gru/runs/ogmpbp0b)
- Số liệu trong báo cáo: TN = 10.810, FP = 1.690, FN = 1.995, TP = 10.505.
- Diễn giải: false negative nhiều hơn false positive 305 mẫu; cần trình bày cả hai loại lỗi thay vì chỉ nêu accuracy.

### Lab 5 — DistilBERT, IMDB

![Confusion matrix DistilBERT Lab 5](./Lab5/outputs/part_a/baseline/figures/confusion_matrix.png)

- Bản lưu trong workspace: [confusion_matrix.png](./Lab5/outputs/part_a/baseline/figures/confusion_matrix.png)
- W&B baseline: [DistilBERT baseline — x0yn9sgf](https://wandb.ai/nmanhcuong1402-dnu/csc4007-lab5-transformer/runs/x0yn9sgf)
- Số liệu trong báo cáo: negative → positive = 900; positive → negative = 1.329; tổng lỗi 2.229/25.000.
- Lưu ý: hai lớp IMDB tương đối cân bằng; nên giải thích thêm precision/recall hoặc loại lỗi, không chỉ so sánh ô đường chéo.

### Lab 5 — PhoBERT, VietNews

![Confusion matrix PhoBERT Lab 5](./Lab5/outputs/vietnews_phobert/figures/confusion_matrix.png)

- Bản lưu trong workspace: [confusion_matrix.png](./Lab5/outputs/vietnews_phobert/figures/confusion_matrix.png)
- Run PhoBERT có `mode=offline`, ID `3beiqk1j`; hiện **không có liên kết run W&B cloud** trong báo cáo.
- Các cặp nhầm lẫn nổi bật theo report: Thời sự → Pháp luật (78), Kinh doanh → Bất động sản (36), Đời sống → Kinh doanh (33), Thời sự → Kinh doanh (28).
- Ảnh này hiện là artefact cục bộ, không nên ghi chú là ảnh tải từ dashboard W&B.

## Ưu tiên 2 — Learning Curves: Train Loss và Validation Loss

### Lab 3 — RNN, IMDB

![Learning curve Lab 3 RNN](./Lab3/outputs/figures/loss_curve.png)

- Bản lưu trong workspace: [loss_curve.png](./Lab3/outputs/figures/loss_curve.png)
- Dữ liệu epoch: [epoch_history.csv](./Lab3/outputs/metrics/epoch_history.csv)
- Các run W&B được lưu offline; report chưa cung cấp run URL online.
- Đọc biểu đồ: validation loss thấp nhất ở epoch 4 rồi tăng ở epoch 5, trong khi train loss bắt đầu tăng sau epoch 4. Đường này thể hiện dao động và mô hình học chưa ổn định; không nên mô tả thành trường hợp train loss giảm liên tục trong khi val loss chỉ tăng.
- Chọn checkpoint theo validation metric/loss phù hợp với quy tắc đã dùng trong run; không mặc định lấy epoch cuối.

### Lab 4 — GRU và các biến thể

![Learning curves Lab 4 từ W&B histories](./Lab4/reports/figures/wandb_loss_curves.png)

- Biểu đồ tổng hợp: [wandb_loss_curves.png](./Lab4/reports/figures/wandb_loss_curves.png)
- Dữ liệu epoch GRU: [epoch_history.csv](./Lab4/outputs/runs/gru_baseline_online_20261015/metrics/epoch_history.csv)
- W&B GRU run: [gru_baseline_online_20261015](https://wandb.ai/nmanhcuong1402-dnu/csc4007-lab4-lstm-gru/runs/ogmpbp0b)
- Quan sát GRU: train loss giảm từ khoảng 0,673 xuống 0,123; validation loss thấp nhất khoảng 0,335 ở epoch 4 rồi tăng lên khoảng 0,394 ở epoch 6. Checkpoint được chọn theo validation macro-F1 ở epoch 5, không phải epoch cuối.
- Biểu đồ tổng hợp trong report có các đường GRU, BiLSTM và Stacked BiGRU; baseline LSTM không có trong chart W&B tổng hợp vì run baseline chưa log online.

### Lab 5 — DistilBERT (tham khảo bổ sung)

![Learning curve DistilBERT Lab 5](./Lab5/outputs/part_a/baseline/figures/loss_curve.png)

- Bản lưu: [loss_curve.png](./Lab5/outputs/part_a/baseline/figures/loss_curve.png)
- W&B baseline online: [x0yn9sgf](https://wandb.ai/nmanhcuong1402-dnu/csc4007-lab5-transformer/runs/x0yn9sgf)
- Report Lab 5 ghi nhận validation loss tăng về cuối trong khi train loss tiếp tục giảm, là dấu hiệu cần kiểm tra overfitting. Dùng validation để chọn checkpoint.

## Ưu tiên 3 — Per-class Recall / F1 / Support cho VietNews

### Lab 1 — UVN-1: biểu đồ phân phối lớp, không phải biểu đồ hiệu năng

Lab 1 chỉ audit dữ liệu, chưa huấn luyện classifier. Vì vậy **không có per-class recall, F1-score hoặc accuracy** để vẽ cho Lab 1. Biểu đồ hợp lệ ở chặng này là **số mẫu/support theo lớp**.

- Bằng chứng hiện có: [UVN-1 audit report](./Lab1/reports/vietnews_audit.md).
- Báo cáo ghi `Kinh doanh`: 1.113/3.268 (34,1%) và `Xe`: 24/3.268 (0,7%), chênh lệch support 46,4 lần.
- Chỉ có hai số lớp này được nêu trong báo cáo đã lưu; chưa có bảng đầy đủ 13 lớp trong artefact workspace để vẽ biểu đồ phân phối hoàn chỉnh. Không nội suy các lớp còn lại.
- Khi có thống kê đầy đủ, vẽ bar chart `label` vs `count`, sắp theo support; đánh dấu rõ đây là **class distribution**, không phải model performance.

### Lab 5 — VietNews PhoBERT: per-class recall / F1 / support

Confusion matrix phía trên cho thấy nhầm lẫn theo lớp, nhưng chart bar riêng theo lớp sẽ dễ đọc hơn với 11 nhãn.

- Dữ liệu dự đoán: [test_predictions.csv](./Lab5/outputs/vietnews_phobert/predictions/test_predictions.csv).
- Metrics tổng hợp công khai: [metrics_summary_public.json](./Lab5/outputs/vietnews_phobert/metrics/metrics_summary_public.json).
- Confusion matrix hiện có: [confusion_matrix.png](./Lab5/outputs/vietnews_phobert/figures/confusion_matrix.png).
- W&B run PhoBERT: offline ID `3beiqk1j`; chưa có dashboard URL online.

**Biểu đồ nên tạo từ test predictions:**

1. Tính theo từng lớp: `support = số nhãn thật`, `recall = TP / support`, `F1 = 2 × precision × recall / (precision + recall)`.
2. Vẽ grouped bar chart cho `recall` và `F1`; đặt `support` trong nhãn/tableside hoặc dùng biểu đồ phụ để không làm méo thang metric.
3. Giữ nguyên thứ tự và tên lớp từ `id2label.json`; ghi số support trên mỗi lớp để lớp ít dữ liệu không bị ẩn.
4. Chỉ tính trên test split đã dùng trong báo cáo; không tính trên train/validation rồi gọi là test performance.

**Lưu ý:** workspace hiện có prediction CSV, nhưng chưa có artefact bar chart per-class. Do đó biểu đồ này cần được sinh từ các cột `y_true` và `y_pred` của file dự đoán; không thay thế bằng macro-F1 tổng thể 0,8447.

## Ma trận trạng thái — cần lấy gì tiếp?

| Biểu đồ | Đã có ảnh trong workspace | W&B cloud | Việc cần làm |
|---|---|---|---|
| Lab 4 GRU confusion matrix | Có | Có run online | Dùng ảnh hiện tại; nếu cần giao diện W&B, mở run và export panel |
| Lab 5 DistilBERT confusion matrix | Có | Có run online | Dùng ảnh hiện tại hoặc export panel từ run `x0yn9sgf` |
| Lab 5 PhoBERT confusion matrix | Có | Run offline | Dùng ảnh local; chỉ sync lên W&B nếu cần dashboard |
| Lab 3 train/val loss | Có | Run offline | Dùng ảnh/CSV local; cần online thì sync run offline rồi kiểm tra chart |
| Lab 4 train/val loss | Có | Có các run online | Dùng chart report hoặc export trực tiếp từ run |
| Lab 1 UVN-1 support theo lớp | Chưa có chart đầy đủ | Không áp dụng | Cần lấy bảng đủ 13 label counts trước khi vẽ |
| Lab 5 PhoBERT per-class recall/F1/support | Chưa có bar chart | Run offline | Tính từ test predictions, sau đó mới vẽ |

## Gợi ý bố cục slide

1. **Confusion matrix:** Lab 4 GRU và Lab 5 DistilBERT/PhoBERT; nêu 2–3 nhầm lẫn lớn nhất.
2. **Learning curves:** Lab 3 RNN và Lab 4 GRU; chỉ ra epoch validation tốt nhất, đoạn validation loss tăng và lý do không lấy epoch cuối.
3. **Class imbalance/per-class:** Lab 1 support UVN-1 và Lab 5 recall/F1/support PhoBERT; nhấn mạnh support ít làm metric theo lớp biến động và accuracy tổng có thể che lớp yếu.
4. Ghi rõ dataset, split, checkpoint và nguồn hình (ảnh W&B online hay artefact local) dưới mỗi chart.

