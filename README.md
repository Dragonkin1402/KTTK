# Báo cáo tổng kết NLP — Lab 1 đến Lab 6

> Báo cáo tổng hợp nội dung, kết quả, lỗi và mối liên hệ giữa sáu lab dựa trên README, báo cáo phân tích và artefact được lưu trong từng thư mục lab. Các con số dưới đây là số liệu ghi trong báo cáo của từng lab; chúng không mặc nhiên tạo thành một benchmark so sánh trực tiếp. Khi dữ liệu, split, cấu hình hoặc nguồn đầu ra khác nhau, báo cáo nêu rõ giới hạn thay vì suy rộng.

## 1. Tóm tắt điều hành

Sáu lab hình thành một tiến trình từ **đánh giá độ tin cậy của dữ liệu** đến **xây dựng mô hình**, rồi chuyển sang **đánh giá câu trả lời có căn cứ**:

1. **Lab 1 — Data audit:** kiểm tra dữ liệu, nhãn, trùng lặp, metadata và nguy cơ leakage trước khi huấn luyện.
2. **Lab 2 — TF-IDF/BoW + mô hình tuyến tính:** thiết lập baseline có thể giải thích và bắt đầu phân tích lỗi thực tế.
3. **Lab 3 — RNN:** chuyển từ vector đặc trưng sang chuỗi token; kết quả cảnh báo rằng có kiến trúc tuần tự không đồng nghĩa với hiệu năng tốt.
4. **Lab 4 — LSTM/GRU:** cải thiện khả năng học chuỗi, chọn checkpoint bằng validation và quản lý các thí nghiệm riêng biệt.
5. **Lab 5 — Transformer:** dùng pretrained model để tăng chất lượng, đồng thời quan sát chi phí tính toán, truncation và overfitting.
6. **Lab 6 — Prompt engineering và RAG:** chuyển trọng tâm từ dự đoán nhãn sang câu trả lời có trích dẫn, kiểm tra được và biết từ chối khi thiếu bằng chứng.

**Kết luận chính:** Lab 2 cho thấy TF-IDF + Logistic Regression là baseline mạnh trên IMDB; RNN cơ bản ở Lab 3 chưa đạt mức đó; GRU ở Lab 4 cải thiện rõ so với LSTM baseline nhưng vẫn thấp hơn TF-IDF trong các kết quả được báo cáo; DistilBERT ở Lab 5 đạt điểm cao nhất trong các số liệu IMDB hiện có, nhưng khác split/cấu hình nên không phải phép so sánh hoàn toàn kiểm soát. Các kết quả VietNews chỉ so sánh được khi cùng dataset và split: smoke-test 30 dòng của Lab 2/3 không thể đối chiếu trực tiếp với bộ 54.967 bài Lab 5 hay UVN-1 3.268 bài Lab 1. Lab 6 chỉ đánh giá sample-output và retrieval offline, **không có live LLM inference**.

---

## 2. Bức tranh xuyên suốt

### Câu hỏi nghiên cứu dịch chuyển như thế nào?

| Chặng | Câu hỏi trung tâm | Đầu ra cần đánh giá |
|---|---|---|
| Lab 1 | Dữ liệu có đủ đáng tin để đưa vào mô hình không? | Schema, missingness, nhãn, duplicate, metadata, split và rủi ro leakage |
| Lab 2 | Baseline văn bản nào hoạt động tốt, và sai ở đâu? | BoW/TF-IDF, Logistic Regression/Linear SVM, confusion matrix, lỗi cụ thể |
| Lab 3 | Chuỗi token và thứ tự từ có giúp hơn đặc trưng túi từ không? | Embedding + RNN, sequence audit, truncation, UNK, learning curves |
| Lab 4 | LSTM/GRU có học phụ thuộc chuỗi tốt hơn RNN cơ bản không? | Ablation trên validation, checkpoint, test metrics, lỗi theo lớp |
| Lab 5 | Pretraining và self-attention cải thiện đến mức nào so với mô hình chuỗi? | Fine-tuning Transformer, tokenization, chi phí, lỗi và giới hạn độ dài |
| Lab 6 | Câu trả lời có chính xác, được hỗ trợ và truy nguyên được không? | Prompt metrics, evidence support, retrieval, citation, refusal, injection checks |

### Những nguyên tắc được kế thừa

- **Audit trước khi tối ưu mô hình:** lỗi dữ liệu hoặc split có thể khiến mọi metric phía sau trở nên sai lệch.
- **Tách train/validation/test:** chọn cấu hình trên validation; chỉ dùng test để đánh giá cuối cùng.
- **Giữ baseline đơn giản:** TF-IDF + mô hình tuyến tính là mốc bắt buộc để biết kiến trúc phức tạp có thực sự đem lại giá trị không.
- **Phân tích lỗi đi cùng metric:** confusion matrix cho biết loại nhầm lẫn; đọc mẫu giúp tìm giả thuyết về phủ định, tương phản, chủ đề giao thoa, độ dài hoặc evidence.
- **Kiểm soát provenance:** ghi rõ nguồn, split, seed, cấu hình, model/run và liệu đầu ra là inference thật hay dữ liệu minh họa.
- **Không đồng nhất “đúng cú pháp” với “đúng ngữ nghĩa”:** JSON hợp lệ, trích đúng substring hay có citation không tự chứng minh dự đoán/câu trả lời được hỗ trợ.

---

## 3. Tổng kết từng lab

### Lab 1 — Data Auditing & Data Card

**Nội dung tập trung**

- Chuẩn hóa schema văn bản/nhãn; kiểm tra missing/empty text, HTML, Unicode, độ dài, phân phối nhãn và duplicate.
- So sánh dữ liệu trước/sau cleaning; dùng Great Expectations để kiểm tra các quy tắc dữ liệu.
- Dùng Cleanlab để xếp hạng mẫu có khả năng có vấn đề về nhãn; ghi lại rủi ro trong Data Card thay vì tự động sửa dữ liệu.
- Chuyển checklist từ review IMDB nhị phân sang tin tiếng Việt UVN-1 đa lớp, có `source` và `publish_date`.

**Bằng chứng và nhận xét**

- Báo cáo audit IMDB ghi 50.000 mẫu cân bằng 25.000/25.000, không thiếu text/label, có **832 dòng thuộc nhóm exact duplicate (1,66%)**, và 11 dòng chứa HTML entity. Cleanlab đánh dấu 1.003/50.000 mẫu (2,01%) cần xem xét; đây là **candidates**, không phải xác nhận nhãn sai.
- Báo cáo UVN-1 ghi 3.268 bài, 13 lớp, không thiếu các trường được kiểm tra; lớp `Kinh doanh` chiếm 34,1% trong khi `Xe` chiếm 0,7% (lệch 46,4 lần). Có 46 dòng thuộc nhóm exact duplicate và 6 cặp near-duplicate trong mẫu dò; 28 bài dài hơn ngưỡng 10.000 ký tự.
- Audit gợi ý random split chưa đủ để đánh giá tổng quát hóa theo nguồn/thời gian. Cần thử split theo bài gốc/sự kiện, theo `source` hoặc theo thời gian trước khi khẳng định có leakage.

**Phân tích lỗi/rủi ro**

- **Nhãn nghi vấn:** Cleanlab hỗ trợ ưu tiên rà soát, không xác minh ngữ nghĩa nhãn. Nếu sửa tự động theo flag, có thể thay nhãn đúng thành nhãn sai.
- **Trùng lặp:** exact duplicate và gần trùng có thể khiến train/test chứa biến thể của cùng bài/review; similarity heuristic chỉ tạo danh sách cần kiểm tra.
- **Mất cân bằng đa lớp:** accuracy có thể che giấu recall rất thấp ở lớp hiếm; cần macro-F1, per-class recall và support.
- **Source/time bias:** metadata là cơ sở thiết kế phép thử, không phải bằng chứng rằng model đã học thuộc nguồn hoặc bị leakage.
- **Ngưỡng độ dài:** cảnh báo bài dài không đồng nghĩa dữ liệu lỗi; cần quyết định theo mục tiêu và không tự động cắt/xóa.

**Vai trò trong chuỗi lab:** đặt nền tảng về chất lượng dữ liệu, split và cách diễn giải bằng chứng; các lab mô hình kế thừa nhưng không thể thay audit bằng một metric cao.

### Lab 2 — Preprocessing, Vectorization & Baseline ML

**Nội dung tập trung**

- Pipeline: `audit → preprocess → vectorize → baseline ML → evaluate → error analysis`.
- So sánh BoW với TF-IDF trên IMDB; báo cáo dùng Logistic Regression, word n-gram 1–2.
- Thử chuyển quy trình sang tiếng Việt bằng TF-IDF word-level, có/không Vietnamese word segmentation; character TF-IDF là cấu hình bonus.
- Giữ dấu câu, không bỏ stopword/stemming tùy tiện vì các tín hiệu như phủ định có thể quan trọng với sentiment.

**Kết quả được ghi nhận**

| Thí nghiệm | Cấu hình | Macro-F1 | Accuracy |
|---|---|---:|---:|
| IMDB I0 | TF-IDF word 1–2 + Logistic Regression | 0,9064 | 0,9064 |
| IMDB I1 | BoW word 1–2 + Logistic Regression | 0,8952 | 0,8952 |
| VietNews V0 | Word TF-IDF + Linear SVM, không segmentation | 1,0000 | 1,0000 |
| VietNews V1 | Word TF-IDF + Linear SVM, có segmentation | 1,0000 | 1,0000 |

Trên IMDB, TF-IDF hơn BoW 0,0112 điểm ở cả hai metric trong báo cáo. Error analysis ghi nhận các nhóm lỗi như mixed sentiment, phủ định, irony/sarcasm, câu dài và từ tích cực cục bộ lấn át đánh giá tổng thể.

**Giới hạn quan trọng của kết quả tiếng Việt**

- Báo cáo Lab 2 nói VietNews sample chỉ có **30 mẫu, 3 lớp**, được dựng theo template và quá dễ; V0/V1 đều không có lỗi. Vì vậy, 1,0000 không chứng minh segmentation hữu ích hay vô ích trên VietNews thật.
- README của Lab 2 mô tả quy trình có thể chạy với VietNewsSense đầy đủ, nhưng báo cáo thực nghiệm hiện ghi kết quả trên tiny sample. Cần ưu tiên provenance trong report/output cụ thể, không suy ra đã chạy dataset lớn chỉ từ hướng dẫn README.
- Báo cáo Lab 2 ghi 824 dòng duplicate IMDB (~1,65%), khác con số 832 ở Lab 1. Hai phép đếm không nên gộp thành một giá trị: có thể khác cách định nghĩa/phiên bản dữ liệu hoặc lần chạy; cần đối chiếu script, fingerprint và artefact trước khi kết luận.

**Giá trị phương pháp:** tạo baseline mạnh, rẻ và dễ tái hiện. Đây là mốc cần giữ khi so sánh các kiến trúc sâu hơn.

### Lab 3 — Embedding + RNN và sequence transfer

**Nội dung tập trung**

- Thay BoW/TF-IDF bằng chuỗi token, embedding học được và hidden state tuần tự.
- Theo dõi train/validation bằng W&B; kiểm tra vocabulary, UNK, `max_len`, truncation và padding.
- Trên IMDB, so sánh với baseline Lab 2; trên VietNewsSense, chỉ audit ảnh hưởng tokenization thay vì huấn luyện thêm một RNN đầy đủ.

**Kết quả được ghi nhận**

- Báo cáo ghi RNN cấu hình tốt nhất có test accuracy **0,6145** và macro-F1 **0,6112**, thấp hơn baseline TF-IDF Lab 2 là 0,9064.
- Sequence audit của một cấu hình `max_len=128` ghi median 196 token, p95 665 token và truncation rate 0,8345; đây là cảnh báo cấu hình làm mất nhiều phần văn bản.
- Báo cáo mô tả loss/validation dao động, có dấu hiệu overfitting/không ổn định; cấu hình 2 đạt validation macro-F1 khoảng 0,5828, trong khi cấu hình tốt nhất đạt khoảng 0,6162.
- Transfer check dùng `sample_vietnews_tiny.csv`: whitespace tạo median 30 token, p95 31; Underthesea median 22, p95 25. Cả hai có UNK/truncation bằng 0 trong mẫu nhỏ; padding ratio lần lượt 0,8828 và 0,9102.

**Phân tích lỗi**

- Các lỗi sentiment được nhóm quanh phủ định, `but/however`, mixed sentiment, sarcasm, tín hiệu đầu/cuối review và khác biệt giữa đánh giá một khía cạnh với nhãn tổng thể.
- Lỗi kỹ thuật/thiết kế nổi bật là `max_len` thấp so với phân phối chuỗi: RNN không thể học thông tin đã bị cắt. Thêm độ phức tạp cho mô hình không bù được dữ liệu đầu vào bị mất.
- Sequence audit tiếng Việt chỉ trả lời tokenizer làm thay đổi độ dài/vocabulary thế nào; không chứng minh tokenizer nào cải thiện phân loại.

**Liên hệ với Lab 2:** Lab 3 là phép thử quan trọng cho giả thuyết “mô hình tuần tự sẽ tốt hơn vì biết thứ tự từ”. Kết quả hiện có bác bỏ kết luận đơn giản đó: RNN cơ bản chưa vượt baseline. Cần cải thiện độ dài chuỗi, huấn luyện và kiến trúc trước khi đánh giá lợi ích biểu diễn.

### Lab 4 — LSTM/GRU và phụ thuộc dài

**Nội dung tập trung**

- Chuyển từ RNN cơ bản sang LSTM/GRU có gates; thử LSTM baseline, GRU, BiLSTM và stacked BiGRU.
- Lưu artefact riêng từng run; chọn mô hình theo validation macro-F1, không dùng test để lựa chọn.
- Đọc learning curves và confusion matrix, thay vì chỉ so sánh accuracy cuối.

**Kết quả được ghi nhận**

| Run | Validation accuracy | Validation macro-F1 | Test accuracy | Test macro-F1 |
|---|---:|---:|---:|---:|
| LSTM baseline | 0,7872 | 0,7871 | 0,7808 | 0,7807 |
| GRU một chiều | **0,8680** | **0,8680** | **0,8526** | **0,8526** |
| BiLSTM | 0,8363 | 0,8359 | — | — |
| Stacked BiGRU | 0,8624 | 0,8621 | — | — |

GRU là mô hình được chọn bằng validation; test confusion matrix có 1.690 false positive và 1.995 false negative trên 25.000 mẫu. Learning curve cho thấy train loss tiếp tục giảm trong khi validation loss tăng về cuối, phù hợp với dấu hiệu overfitting nhẹ; checkpoint tốt nhất xuất hiện trước epoch cuối.

**Phân tích lỗi**

- Các trường hợp đọc thủ công gồm review vừa khen một khía cạnh vừa chê tổng thể, sentiment đảo chiều sau kỳ vọng ban đầu, từ/cụm phủ định và review dài có tín hiệu trái chiều.
- Đây là diễn giải giả thuyết từ các mẫu, không phải chứng minh nguyên nhân nhân quả; confidence khoảng 0,67 trong các lỗi được xem cũng nhắc rằng dự đoán sai không nhất thiết đi kèm độ chắc chắn thấp.
- Ablation chưa phải thí nghiệm một yếu tố duy nhất: các run thay đổi đồng thời kiến trúc, chiều chạy, dropout và/hoặc số tầng. Vì vậy mức cải thiện không thể quy toàn bộ cho riêng “GRU”, “bidirectional” hay “thêm layer”.

**Liên hệ với Lab 3:** GRU cải thiện mạnh so với RNN của Lab 3 trong các kết quả được báo cáo, nhưng so sánh giữa hai lab chưa chắc cùng split/cấu hình. Quan trọng hơn, Lab 4 dùng validation nhất quán để chọn checkpoint, một cải tiến về quy trình đánh giá chứ không chỉ về kiến trúc.

### Lab 5 — Fine-tuning Transformer

**Nội dung tập trung**

- Fine-tune DistilBERT cho sentiment IMDB; so sánh `max_length=256` và `128`.
- Dùng PhoBERT cho phân loại chủ đề tiếng Việt; so sánh với TF-IDF + Linear SVM trên split được báo cáo là cố định.
- Theo dõi W&B, truncation, learning curves, confusion matrix và chi phí thời gian/tính toán.

**Kết quả được ghi nhận**

| Thí nghiệm | Cấu hình/kết quả | Macro-F1 | Accuracy |
|---|---|---:|---:|
| IMDB DistilBERT | `max_length=256`, test | **0,91081** | **0,91084** |
| IMDB DistilBERT | `max_length=128`, test | 0,87904 | 0,87904 |
| VietNews TF-IDF + Linear SVM | test split theo báo cáo Lab 5 | 0,79610 | 0,79753 |
| VietNews PhoBERT | cùng test split theo báo cáo Lab 5 | **0,84472** | **0,84646** |

- Trên IMDB, cấu hình 256 cao hơn cấu hình 128 khoảng 3,18 điểm phần trăm macro-F1; tokenization audit ước tính truncation 42,95% ở 256 và 89,15% ở 128. Đây là so sánh có cùng model/cấu hình chính và thay đổi độ dài, nên hỗ trợ giả thuyết rằng giới hạn quá ngắn làm giảm hiệu quả; không chứng minh rằng cứ tăng `max_length` mãi sẽ tiếp tục cải thiện.
- DistilBERT mất khoảng 2 giờ 39 phút cho baseline trên thiết bị MPS theo báo cáo.
- Với VietNews, PhoBERT hơn TF-IDF/SVM khoảng 4,86 điểm phần trăm macro-F1, nhưng run PhoBERT khoảng 5 giờ 32 phút trên CPU. Các nhầm lẫn nổi bật gồm Thời sự/Pháp luật, Kinh doanh/Bất động sản, Đời sống/Kinh doanh và Thời sự/Kinh doanh.

**Giới hạn và vấn đề cần đối chiếu**

- Bảng IMDB Lab 5 đối chiếu với Lab 4 có ghi rõ phân bổ train/validation khác nhau; vì vậy chênh lệch DistilBERT–GRU chỉ mang tính tham khảo, không phải benchmark kiểm soát hoàn toàn.
- Báo cáo Lab 5 ghi VietNews 54.967 bài, 11 lớp và nói dùng cùng split Lab 2; nhưng báo cáo Lab 2 trong workspace mô tả kết quả VietNews trên tiny sample 30 dòng/3 lớp. Lab 1 lại mô tả UVN-1 3.268 bài/13 lớp. Đây là các mô tả dataset khác nhau hoặc provenance chưa đồng bộ; do đó **không nên công bố phép so sánh Lab 2–Lab 5 như cùng tập test nếu chưa xác minh split ID, dataset fingerprint và nhãn**.
- Nguồn/phiên bản ngoài của CSV VietNews Lab 5 chưa được xác minh độc lập. Kết quả chỉ áp dụng cho dữ liệu và split được ghi trong báo cáo.

**Liên hệ với Lab 4:** Transformer dùng pretrained representation và self-attention, giảm phụ thuộc vào học embedding từ đầu; kết quả IMDB cao hơn các số liệu Lab 4 hiện có nhưng kèm chi phí lớn hơn. Lab 5 tiếp tục cùng nguyên tắc: chọn bằng validation, giữ test cuối cùng và phân tích lỗi thay vì chỉ nêu điểm số.

### Lab 6 — Prompt Engineering và RAG

**Nội dung tập trung**

- Thiết kế prompt baseline, prompt cải tiến và single-hop RAG với retriever TF-IDF/cosine.
- Chấm sentiment, JSON schema, exact evidence, semantic support; thử retrieval `top_k=1` và `3`.
- Kiểm tra citation, no-evidence refusal và prompt injection.

**Kết quả được ghi nhận**

- Chế độ chạy là **sample-output cùng retrieval/evaluation offline**; không có live LLM inference, không dùng API credential. Dataset và KB là synthetic teaching examples, không phải review IMDB thật.
- Trên 10 eval outputs được cấp: v1 đạt sentiment 5/10, JSON hợp lệ 8/10, exact evidence 7/10, evidence được hỗ trợ ngữ nghĩa 1/10; v2 lần lượt 9/10, 10/10, 10/10 và 3/10.
- Các điểm số này đo **sample outputs**, không phải hiệu quả của prompt v1/v2 vừa soạn. Vì không có inference mới từ prompt đó, không thể kết luận prompt v2 đã gây ra cải thiện.
- Với bốn câu có gold QA, Hit@1 và Hit@3 đều 4/4 nếu tính có ít nhất một gold chunk; tuy nhiên k=1 lấy đủ các gold chunks chỉ 2/6, còn k=3 lấy đủ 6/6. k=3 có thể bổ sung chunk nhiễu.
- Một câu trả lời mẫu dùng citation sai chunk; một số câu có citation hợp lệ nhưng nội dung chung chung, không trả lời câu hỏi. Câu hỏi thiếu bằng chứng được từ chối trong sample; đó không phải chứng nhận bảo vệ injection trên một mô hình đang chạy.

**Phân tích lỗi**

- Lỗi prompt-output mẫu gồm bỏ sót phủ định/đối lập, chọn lời khen cục bộ thay vì sentiment tổng thể, JSON không hợp lệ, evidence không có trong review hoặc evidence có thật nhưng không hỗ trợ nhãn.
- Lỗi RAG có thể nằm ở retrieval (bỏ sót chunk, thêm nhiễu), generation (trả lời không khớp câu hỏi), hoặc citation (sai ID/ngoài context). Chỉ kiểm tra citation có tồn tại là chưa đủ; phải kiểm tra claim được chunk hỗ trợ.
- Bộ QA nhỏ và gold có cho bốn câu nên mọi metric chỉ có giá trị trong phạm vi dữ liệu thực hành.

**Liên hệ với các lab trước:** Lab 6 quay lại bài toán sentiment nhưng chuyển tiêu chí chất lượng từ “phân loại đúng” sang “trả lời đúng, có bằng chứng và truy nguyên được”. Bài học về error analysis, provenance và giới hạn kết luận từ Lab 1–5 vẫn cần thiết; RAG không tự động khắc phục lỗi của dữ liệu, retriever hay mô hình sinh.

---

## 4. Lab sau cải thiện gì so với lab trước?

Phần này đối chiếu trực tiếp từng cặp lab liền kề. “Cải thiện” ở đây có hai nghĩa cần tách biệt:

- **Cải thiện năng lực/phương pháp:** lab sau bổ sung một bước, biểu diễn hoặc cách đánh giá mà lab trước chưa có.
- **Khắc phục đã được chứng minh:** kết quả thực nghiệm xác nhận điểm yếu trước đó đã giảm trên dữ liệu và giao thức tương đương.

Không phải thay đổi nào cũng đạt nghĩa thứ hai. Một lab có thể đưa ra phương pháp nhằm xử lý hạn chế của lab trước, nhưng nếu dữ liệu/split khác hoặc chưa có thí nghiệm đối chứng thì chỉ nên nói là **đã mở rộng/có hướng khắc phục**, không tuyên bố đã giải quyết triệt để.

### Lab 1 → Lab 2: từ biết dữ liệu có vấn đề sang đo được baseline mô hình

**Điểm Lab 1 để lại**

- Lab 1 xác định schema, missingness, duplicate, phân phối nhãn, metadata và các dấu hiệu có thể gây leakage; nhưng đây là lab audit, **chưa trả lời dữ liệu sau audit có dự đoán được nhãn tốt đến đâu**.
- Các cặp near-duplicate và Cleanlab flags vẫn cần người xem xét. Audit phát hiện/rank vấn đề chứ chưa cung cấp bằng chứng rằng các cờ đã được sửa hoặc ảnh hưởng bao nhiêu đến metric.
- Random split vẫn có thể đưa các bài/review cùng nguồn hoặc cùng sự kiện vào cả train và test.

**Lab 2 bổ sung gì**

1. Đưa dữ liệu vào pipeline thực nghiệm hoàn chỉnh: preprocessing → vectorizer → classifier → test metrics → error analysis.
2. Tạo mốc định lượng bằng BoW và TF-IDF, thay vì chỉ mô tả chất lượng đầu vào.
3. Gắn lỗi dự đoán với hiện tượng văn bản cụ thể như phủ định, mixed sentiment và từ tích cực cục bộ.
4. Bắt đầu kiểm tra transfer: cùng ý tưởng pipeline nhưng thử word segmentation cho tiếng Việt.

**Khắc phục được đến đâu**

- Lab 2 trả lời được câu hỏi “baseline hiện tại đạt gì?”; trên IMDB, TF-IDF có macro-F1 0,9064 so với BoW 0,8952 trong các cấu hình đã báo cáo.
- Error analysis giúp nối lỗi mô hình với ví dụ văn bản, là bước tiếp theo sau việc audit thống kê.
- Tuy nhiên, Lab 2 **không tự khắc phục** duplicate/leakage hay nhãn nghi vấn của Lab 1. Kết quả chỉ đáng tin khi các rủi ro đó được xử lý hoặc được phản ánh trong split. Chênh lệch duplicate 832 ở Lab 1 và 824 ở Lab 2 cũng chưa được giải thích bằng fingerprint/cùng tiêu chí đếm.
- Với tiếng Việt, kết quả 1,0 đến từ sample 30 dòng; vì thế transfer check mới thể hiện pipeline chạy được, chưa giải quyết được câu hỏi segmentation trên dữ liệu đại diện.

**Kết luận cặp lab:** Lab 2 tiến từ *đánh giá dữ liệu* sang *đánh giá mô hình trên dữ liệu*, nhưng độ tin cậy của mô hình vẫn bị giới hạn bởi những rủi ro dữ liệu Lab 1 đã phát hiện mà chưa có bằng chứng đã xử lý.

### Lab 2 → Lab 3: từ đặc trưng từ độc lập sang chuỗi có thứ tự

**Điểm Lab 2 để lại**

- TF-IDF/BoW biểu diễn văn bản bằng trọng số/ngữ cảnh n-gram cục bộ; không duy trì biểu diễn tuần tự đầy đủ hay trạng thái ngữ cảnh xuyên suốt review.
- Các lỗi phủ định, tương phản, mixed sentiment và review dài gợi ý rằng tổng hợp tín hiệu từ có thể làm mất quan hệ giữa các mệnh đề.
- Baseline tốt nhưng không cho biết liệu mô hình có khai thác thứ tự từ sẽ xử lý các lỗi đó tốt hơn hay không.

**Lab 3 bổ sung gì**

1. Chuyển input sang chuỗi token, embedding và hidden state RNN để mô hình đọc token theo thứ tự.
2. Thêm sequence audit: đo vocabulary, UNK, độ dài, truncation và padding, những yếu tố không thể hiện đầy đủ trong bảng TF-IDF.
3. Theo dõi train/validation qua epoch và W&B, giúp quan sát học chưa hội tụ hoặc overfit thay vì chỉ nhìn một điểm test.
4. Định nghĩa một phép thử tiếng Việt hẹp hơn: đo tokenizer thay đổi hình dạng đầu vào ra sao, không suy luận trực tiếp chất lượng phân loại từ sequence statistics.

**Khắc phục được đến đâu**

- Lab 3 xử lý được **hạn chế biểu diễn** của Lab 2 về thứ tự: RNN có khả năng mã hóa chuỗi thay vì chỉ cộng/trọng số hóa feature.
- Nhưng kết quả thực nghiệm chưa cho thấy ưu thế: RNN đạt test macro-F1 0,6112 trong báo cáo, thấp hơn baseline TF-IDF 0,9064. Do split/cấu hình không được xác nhận là hoàn toàn tương đương, đây là cảnh báo chứ không phải chứng minh TF-IDF luôn tốt hơn RNN.
- Sequence audit phát hiện nguyên nhân cấu hình đáng chú ý: `max_len=128` trong một run cắt 83,45% chuỗi. Khi nội dung bị cắt, khả năng mô hình hóa thứ tự không thể bù lại thông tin đã mất.
- Do đó Lab 3 **mở ra cách biểu diễn giàu cấu trúc hơn nhưng chưa khắc phục được lỗi dự đoán của Lab 2**; nó đồng thời cho thấy kiến trúc tuần tự cần thiết kế input và huấn luyện phù hợp.

**Kết luận cặp lab:** Lab 3 mở rộng *mô hình có thể biểu diễn điều gì*, nhưng kết quả thấp nhắc rằng representation tốt hơn về lý thuyết không đảm bảo mô hình học tốt hơn trong thực nghiệm.

### Lab 3 → Lab 4: dùng cổng LSTM/GRU và quy trình chọn mô hình tốt hơn

**Điểm Lab 3 để lại**

- RNN cơ bản cập nhật trạng thái theo từng bước; chuỗi dài và phụ thuộc xa có thể khó học, nhất là khi review dài hoặc thông tin quyết định nằm ở cuối.
- Lab 3 ghi nhận validation dao động/overfitting và truncation đáng kể ở `max_len=128`.
- So sánh baseline-versus-RNN cho biết mô hình RNN chưa vượt baseline, nhưng chưa chỉ rõ một kiến trúc gated có cải thiện hay không.

**Lab 4 bổ sung gì**

1. Thay RNN thường bằng LSTM/GRU có gates để điều tiết thông tin qua chuỗi và hỗ trợ học phụ thuộc dài.
2. Thử các biến thể LSTM, GRU, BiLSTM và stacked BiGRU.
3. Chuẩn hóa quy trình lựa chọn: lập bảng validation, chọn theo validation macro-F1, rồi mới dùng test cho run được chọn.
4. Lưu checkpoint/artefact theo từng run; dùng learning curves và confusion matrix để xem overfitting và loại nhầm lẫn.

**Khắc phục được đến đâu**

- So với LSTM baseline Lab 4 (test macro-F1 0,7807), GRU được chọn đạt 0,8526; validation macro-F1 tăng từ 0,7871 lên 0,8680. Đây là bằng chứng GRU tốt hơn **các cấu hình Lab 4 được thử**, không phải khẳng định mọi GRU đều tốt hơn mọi RNN/LSTM.
- Quy trình chọn checkpoint trên validation khắc phục một điểm yếu về phương pháp: không chọn kiến trúc bằng test và không mặc định epoch cuối là tốt nhất.
- So với RNN Lab 3, GRU có điểm test cao hơn nhiều theo báo cáo (0,8526 so với 0,6112), nhưng chưa thể quy toàn bộ mức tăng cho gates vì cần xác nhận cùng split, seed, preprocessing và training budget.
- Lab 4 chưa khắc phục hết overfitting; validation loss vẫn tăng ở các epoch cuối. Ablation cũng đổi nhiều yếu tố đồng thời, nên chưa cô lập được tác động riêng của gates, bidirectionality, số tầng hay dropout.
- `max_len` vẫn là yếu tố cần kiểm soát; đổi kiến trúc không thay thế việc chọn độ dài phù hợp.

**Kết luận cặp lab:** Lab 4 bổ sung kiến trúc phù hợp hơn cho chuỗi và quy trình chọn checkpoint đáng tin hơn. Bằng chứng GRU cải thiện rõ trong các run hiện có, nhưng việc cải thiện so với Lab 3 chỉ nên xem là gợi ý cho tới khi chạy đối chứng cùng giao thức.

### Lab 4 → Lab 5: từ học biểu diễn chuỗi từ đầu sang fine-tuning mô hình pretrained

**Điểm Lab 4 để lại**

- LSTM/GRU phải học embedding và đặc trưng từ dữ liệu của bài toán; dù có gates, chúng vẫn có thể khó học các quan hệ phức tạp nếu dữ liệu/huấn luyện hạn chế.
- GRU Lab 4 tốt hơn LSTM baseline nhưng test macro-F1 0,8526 vẫn thấp hơn TF-IDF Lab 2 và còn khoảng cách với mục tiêu hiệu năng cao.
- Review dài vẫn gây vấn đề; tăng chiều dài chuỗi làm tăng tính toán, còn cắt ngắn làm mất thông tin.

**Lab 5 bổ sung gì**

1. Dùng DistilBERT pretrained cho sentiment IMDB: mô hình bắt đầu từ biểu diễn ngôn ngữ đã học trước, rồi fine-tune cho nhãn cụ thể.
2. Dùng self-attention để mỗi token có thể tương tác trực tiếp với token khác trong giới hạn chuỗi, thay vì chỉ truyền trạng thái tuần tự qua từng bước.
3. So sánh `max_length=128` và `256`, có tokenization audit gắn kết lựa chọn độ dài với truncation.
4. Mở rộng sang tiếng Việt bằng PhoBERT, thay vì chỉ đo sequence statistics như Lab 3.
5. Tiếp tục theo dõi learning curves, validation và chi phí thời gian/tài nguyên.

**Khắc phục được đến đâu**

- Trên IMDB, DistilBERT `max_length=256` đạt test macro-F1 0,91081; cấu hình 128 đạt 0,87904. Cặp này cho thấy tăng giới hạn từ 128 lên 256 có ích trong thí nghiệm cụ thể, đồng thời tỷ lệ truncation audit giảm từ 89,15% xuống 42,95%.
- PhoBERT trong báo cáo Lab 5 hơn TF-IDF/SVM trên test được báo cáo khoảng 4,86 điểm phần trăm macro-F1. Đây là kết quả cho split đó, không nên gán cho mọi dữ liệu tiếng Việt.
- Cải thiện chất lượng đi cùng chi phí: run PhoBERT mất khoảng 5 giờ 32 phút trên CPU; Transformer cũng có vấn đề overfitting và giới hạn độ dài, nên không xóa bỏ các trade-off của Lab 4.
- So sánh Lab 5 với Lab 4 chưa hoàn toàn công bằng do khác phân bổ train/validation. Báo cáo Lab 5 cũng nêu các quy mô VietNews khác Lab 2/1; cần xác minh dataset/split trước khi xem đây là sửa lỗi của baseline trước.

**Kết luận cặp lab:** Lab 5 nâng cấp nguồn biểu diễn bằng pretraining và cho kết quả cao hơn trong các số liệu được báo cáo, đồng thời đo được tác động của độ dài. Nhưng đây là cải thiện chất lượng với chi phí lớn hơn, chưa phải chứng minh Transformer thắng trong một benchmark đồng nhất.

### Lab 5 → Lab 6: từ dự đoán nhãn sang câu trả lời có căn cứ và truy nguyên

**Điểm Lab 5 để lại**

- Fine-tuning Transformer giải quyết phân loại nhãn, nhưng một nhãn/dự đoán không nhất thiết giải thích được vì sao model đưa ra kết luận.
- Confusion matrix và error analysis hỗ trợ phân tích sau dự đoán, nhưng không buộc hệ thống phải trích đoạn nguồn hỗ trợ từng claim.
- Mô hình pretrained có thể tạo đầu ra thuyết phục nhưng sai; metric phân loại không đánh giá khả năng trả lời QA, từ chối khi thiếu evidence hay độ chính xác citation.

**Lab 6 bổ sung gì**

1. Chuyển từ dự đoán nhãn đơn sang đầu ra có schema, evidence, giải thích ngắn và confidence; đánh giá riêng accuracy, JSON validity, exact evidence và supported evidence.
2. Thêm retriever TF-IDF/cosine, `top_k`, chunk ID và nguồn để câu trả lời RAG có provenance.
3. Phân loại lỗi thành retrieval, generation, citation và support; kiểm tra câu hỏi không có bằng chứng, từ chối và prompt injection.
4. Nêu rõ mode inference, sample ID, dữ liệu, prompt và giới hạn trong manifest, tránh trộn output mẫu với output chạy thật.

**Khắc phục được đến đâu**

- Lab 6 mở rộng **tiêu chuẩn đánh giá** từ “đúng sentiment” sang “đúng, có bằng chứng, citation kiểm chứng được và từ chối phù hợp”. Đây là lớp kiểm soát mà Lab 5 không có.
- Sample đánh giá cho thấy vì sao các tiêu chí phải tách biệt: v2 sample đạt JSON 10/10 và exact evidence 10/10, nhưng semantic support chỉ 3/10. Đúng format hoặc có trích dẫn nguyên văn chưa đủ để kết luận đúng.
- Retrieval top-3 tìm đủ gold chunks 6/6 trong các câu có gold được báo cáo, tốt hơn top-1 là 2/6; nhưng k lớn có thể thêm nhiễu và generation mẫu vẫn có câu trả lời không hỗ trợ/sai citation.
- Lab 6 **không cải thiện hay thay thế mô hình phân loại Lab 5**; nó thay đổi loại tác vụ và cách kiểm chứng.
- Vì không có live inference, synthetic data và gold QA giới hạn, Lab 6 chưa chứng minh prompt/RAG đã khắc phục lỗi trên người dùng hoặc dữ liệu thật, cũng chưa chứng nhận khả năng chống injection thực tế.

**Kết luận cặp lab:** Lab 6 không phải “Transformer tốt hơn”, mà là một bước từ *dự đoán* sang *trả lời có căn cứ*. Nó cải thiện khả năng truy nguyên/đánh giá ở cấp hệ thống trong thiết kế, nhưng hiệu quả thực tế cần được xác nhận bằng inference thật trên bộ test đủ lớn.

### Bảng chốt: vấn đề trước đó, phản hồi của lab sau và phần còn tồn tại

| Chuyển tiếp | Hạn chế được kế thừa | Lab sau bổ sung/cải thiện | Mức khắc phục theo bằng chứng hiện có | Còn tồn tại |
|---|---|---|---|---|
| Lab 1 → 2 | Chưa có baseline dự đoán hoặc error analysis mô hình | TF-IDF/BoW, classifier, metrics và phân tích lỗi | Đã tạo baseline định lượng trên IMDB | Duplicate/label/leakage chưa chứng minh đã xử lý; tiếng Việt chỉ dùng tiny sample |
| Lab 2 → 3 | Bag-of-words không giữ chuỗi đầy đủ; lỗi ngữ cảnh chưa được kiểm nghiệm bằng mô hình tuần tự | Embedding + RNN; audit UNK, padding, truncation | Mở rộng khả năng biểu diễn, nhưng RNN báo cáo chưa vượt baseline | Hiệu năng thấp, truncation cao, split chưa chắc so sánh được |
| Lab 3 → 4 | RNN cơ bản khó học phụ thuộc dài; training không ổn định | LSTM/GRU, ablation, validation checkpoint, per-run artifacts | GRU tốt nhất hơn LSTM baseline trong Lab 4 | Overfitting còn; ablation nhiều biến; đối chứng với Lab 3 chưa kiểm soát |
| Lab 4 → 5 | Học embedding từ đầu; còn giới hạn về ngữ cảnh/hiệu năng | Pretrained Transformer, self-attention, length ablation, PhoBERT | Điểm cao hơn ở các kết quả Lab 5; `max_length=256` tốt hơn 128 | Compute tốn kém; split/dataset chưa thống nhất với lab trước |
| Lab 5 → 6 | Nhãn dự đoán không bảo đảm giải thích/citation; chưa có QA/no-evidence evaluation | Prompt metrics, RAG, chunk provenance, refusal/injection checks | Mở rộng khả năng kiểm chứng; sample làm lộ khoảng cách format–support | Không live inference; synthetic data; chưa chứng minh cải thiện production |

---

## 5. So sánh phương pháp xuyên các lab

### 4.1 IMDB: từ baseline đến mô hình ngôn ngữ pretrained

| Phương pháp | Biểu diễn | Metric ghi nhận | Nhận xét có thể rút ra |
|---|---|---|---|
| Lab 2 TF-IDF + Logistic Regression | Tần suất có trọng số, word n-gram | Test Macro-F1 0,9064 | Baseline mạnh, nhanh và dễ kiểm tra; không mô hình hóa thứ tự dài |
| Lab 2 BoW + Logistic Regression | Tần suất từ | Test Macro-F1 0,8952 | Thấp hơn TF-IDF trong cấu hình đã thử |
| Lab 3 RNN | Chuỗi token + embedding học từ đầu | Test Macro-F1 0,6112 | Chưa vượt baseline; cấu hình/huấn luyện/độ dài chuỗi là điểm yếu |
| Lab 4 GRU | Chuỗi token + gated recurrent unit | Test Macro-F1 0,8526 | Tốt hơn LSTM baseline trong các run Lab 4; vẫn dưới TF-IDF Lab 2 theo số hiện có |
| Lab 5 DistilBERT | Tokenizer + pretrained Transformer | Test Macro-F1 0,91081 (`max_length=256`) | Điểm cao nhất trong số liệu được báo cáo; chi phí tính toán lớn hơn |

**Cảnh báo so sánh:** các con số là kết quả từng báo cáo, không phải một benchmark thống nhất. Lab 4/Lab 5 ghi nhận khác biệt split; Lab 3 có cấu hình và vấn đề truncation riêng. Không nên dùng chênh lệch tuyệt đối để kết luận kiến trúc nào luôn tốt hơn nếu chưa chạy lại cùng dataset version, split fingerprint, seed, preprocessing và quy tắc chọn checkpoint.

### 4.2 Tiếng Việt: không gộp các tập dữ liệu khác nhau

| Báo cáo/nguồn | Quy mô được ghi | Ý nghĩa |
|---|---:|---|
| Lab 1 UVN-1 | 3.268 bài, 13 lớp | Audit data; có metadata source/date, mất cân bằng lớp |
| Lab 2/3 sample VietNews | 30 mẫu, 3 lớp | Smoke-test/template; V0/V1 đạt 1,0 nhưng không đủ đại diện |
| Lab 5 VietNews | 54.967 bài, 11 lớp | PhoBERT và SVM theo báo cáo Lab 5; nguồn ngoài chưa xác minh |

Không có cơ sở để coi ba bộ trên là cùng một dataset hay cùng split chỉ vì đều là tin tiếng Việt. Trước khi so sánh cần lưu dataset version/fingerprint, mapping lớp, IDs train/validation/test và preprocessing. Transfer checklist vẫn có giá trị: segmentation ảnh hưởng vocabulary/sequence length; class imbalance cần macro-metrics; metadata source/time tạo cơ hội đánh giá held-out.

### 4.3 Phương pháp không chỉ khác về kiến trúc

- **BoW/TF-IDF:** đơn giản, có thể kiểm tra trọng số từ, chi phí thấp; yếu hơn khi nhãn phụ thuộc vào phạm vi phủ định, thứ tự xa hoặc ngữ cảnh.
- **RNN/LSTM/GRU:** biểu diễn thứ tự và ngữ cảnh tuần tự; nhạy với tokenizer, `max_len`, padding, UNK, tối ưu hóa và overfitting.
- **Transformer pretrained:** tận dụng biểu diễn học trước và self-attention; đòi hỏi tài nguyên, quản lý tokenizer/độ dài và đánh giá chi phí.
- **Prompt-only:** cho phép điều khiển nhiệm vụ và schema qua hướng dẫn nhưng đầu ra có thể sai, thiếu ổn định, hoặc tạo evidence không được hỗ trợ.
- **RAG:** thêm bước truy hồi và provenance; cải thiện khả năng kiểm chứng chỉ khi retrieval đủ liên quan, generation trung thành với context và citation hợp lệ. Similarity cao không đồng nghĩa evidence đúng.

---

## 6. Tổng hợp lỗi và nguyên nhân gốc

### A. Lỗi dữ liệu và đánh giá

1. **Duplicate/leakage chưa được xử lý ở mức nhóm:** exact duplicate và near-duplicate có thể lọt qua random split. Hướng xử lý là kiểm tra/nhóm theo nội dung, URL, bài gốc hoặc sự kiện rồi mới chia.
2. **Nhãn nghi vấn bị hiểu nhầm là nhãn sai:** Cleanlab chỉ xếp hạng ứng viên; cần người review và guideline nhãn trước khi sửa.
3. **Metric tổng che lớp hiếm:** đặc biệt ở UVN-1 đa lớp với tỷ lệ lớp lớn nhất/nhỏ nhất 46,4. Cần báo cáo macro-F1, per-class recall và support.
4. **Provenance VietNews không thống nhất:** các báo cáo nêu ba quy mô/lớp khác nhau; việc so sánh chéo cần xác minh dataset/split thực tế.

### B. Lỗi representation và mô hình

1. **Bag-of-words bỏ qua cấu trúc ngữ nghĩa:** từ khen có thể lấn át câu kết luận chê, hoặc phủ định làm đảo polarity.
2. **Truncation:** Lab 3 ghi 83,45% ở `max_len=128`; Lab 5 ghi 89,15% ở 128 và 42,95% ở 256 trên audit riêng. Đây là hai audit/cấu hình khác nhau, không gộp tỷ lệ; cả hai đều cho thấy lựa chọn độ dài cần căn cứ phân phối.
3. **Overfitting:** train loss giảm trong khi validation loss tăng/metric dao động ở RNN/LSTM/GRU/Transformer. Dùng checkpoint tốt nhất validation, early stopping và regularization; không chọn theo test.
4. **Thí nghiệm nhiều biến cùng lúc:** nếu đổi architecture, dropout, layers và chiều chạy cùng lúc thì chỉ biết cấu hình tổng thể tốt hơn, không biết yếu tố nào gây khác biệt.
5. **Số liệu không đồng nhất qua lab:** baseline của Lab 2, RNN Lab 3, GRU Lab 4 và DistilBERT Lab 5 có split và quy trình khác nhau; kết quả cần được coi là mốc tham khảo cho tới khi tái lập trên cùng giao thức.

### C. Lỗi prompt/RAG

1. **Phân loại cục bộ thay vì toàn thể:** praise/criticism ở một khía cạnh không nhất thiết phản ánh sentiment toàn review.
2. **Evidence đúng substring nhưng sai sức hỗ trợ:** exact match kiểm tra sự hiện diện văn bản; cần thêm đánh giá thủ công/semantic support.
3. **Retrieval sai hoặc thiếu:** top-k có đánh đổi giữa recall và nhiễu; kiểm tra gold chunks, không chỉ similarity.
4. **Citation sai hoặc không đủ:** chunk ID phải tồn tại trong context thực sự đưa cho model, đúng nguồn và hỗ trợ chính xác claim.
5. **Đầu ra mẫu bị mô tả nhầm là inference:** Lab 6 đã khai báo sample-output; mọi số liệu phải giữ provenance này để tránh báo cáo sai thực nghiệm.

---

## 7. Đánh giá tổng thể

### Điểm mạnh

- Lộ trình học có tính kế thừa: audit → baseline → sequence → gated RNN → Transformer → RAG.
- IMDB được giữ làm trục so sánh cho nhiều phương pháp; VietNews được dùng như bài toán transfer để nhấn mạnh khác biệt ngôn ngữ/miền dữ liệu.
- Có thói quen dùng macro-F1, confusion matrix, validation, learning curves và error analysis thay vì chỉ tối ưu accuracy.
- Các báo cáo ngày càng minh bạch hơn về run, checkpoint, W&B, chi phí, nguồn dữ liệu, dữ liệu synthetic và artefact không công khai.
- Lab 6 phân biệt đúng giữa định dạng/evidence lexical và hỗ trợ ngữ nghĩa, đồng thời tách retrieval errors khỏi generation errors.

### Hạn chế cần khắc phục

- Một số kết quả tiếng Việt dựa trên tiny sample; một số báo cáo khác dùng bộ lớn nhưng provenance/split chưa đồng bộ với Lab 2.
- Các metric xuyên lab chưa được tạo trên một benchmark chung; so sánh trực tiếp có nguy cơ quy nhầm cải thiện cho kiến trúc.
- Audit duplicate/near-duplicate chưa đồng nghĩa đã loại leakage khỏi split.
- Các ablation của Lab 4 thay đổi nhiều yếu tố cùng lúc; cần thiết kế one-factor-at-a-time hoặc factorial có kiểm soát.
- Phân tích lỗi nhiều trường hợp là mô tả định tính; muốn kết luận mạnh hơn cần schema error bucket nhất quán, đếm tần suất, nhiều người gán nhãn và đo agreement.
- RAG Lab 6 chưa có inference thật và tập QA còn nhỏ; chưa thể đánh giá độ tin cậy trong triển khai.

### Đánh giá theo mức độ bằng chứng

- **Bằng chứng tương đối mạnh trong phạm vi report:** TF-IDF hơn BoW trong hai cấu hình IMDB Lab 2; `max_length=256` hơn 128 trong thí nghiệm DistilBERT Lab 5; GRU là cấu hình validation tốt nhất trong bốn run Lab 4.
- **Bằng chứng gợi ý nhưng không kết luận tổng quát:** GRU/Transformer hơn RNN hoặc mô hình tuyến tính giữa các lab; PhoBERT hơn SVM trong báo cáo Lab 5; segmentation ảnh hưởng đầu vào chuỗi.
- **Không đủ bằng chứng:** segmentation có lợi trên dữ liệu tiếng Việt thật dựa trên sample 30 dòng; prompt v2 đã cải thiện inference thật; RAG đã chống injection an toàn; các kết quả VietNews từ các lab dùng cùng dataset/split.

---

## 8. Hướng cải thiện và quy trình thực nghiệm đề xuất

1. **Đóng băng dữ liệu và giao thức:** ghi dataset source/version, checksum/fingerprint, label mapping, split IDs, seed và preprocessing trong manifest chung.
2. **Kiểm tra leakage trước benchmark:** phát hiện exact/near duplicates; nếu dữ liệu có nguồn/ngày/sự kiện, báo cáo random split cùng source-held-out/time-based/grouped split.
3. **Tạo benchmark IMDB thống nhất:** chạy TF-IDF, RNN, GRU và DistilBERT trên cùng train/validation/test; dùng cùng metric, quy tắc chọn checkpoint và test protocol.
4. **Bóc tách yếu tố mô hình:** thay một tham số mỗi thí nghiệm (ví dụ chỉ `max_len`, chỉ bidirectional hoặc chỉ dropout); giữ log đầy đủ và so sánh nhiều seed nếu khả thi.
5. **Phân tích lỗi có hệ thống:** định nghĩa taxonomy phủ định, contrast, mixed sentiment, sarcasm, lỗi cục bộ/toàn thể, độ dài và nhãn mơ hồ; lấy mẫu có chiến lược, ghi confidence và phân biệt lỗi dữ liệu với giới hạn mô hình.
6. **Đánh giá tiếng Việt trên dữ liệu đủ lớn:** xác minh phiên bản, dùng segmentation và char/word features trên cùng split; báo cáo macro-F1, per-class recall, confusion matrix, thời gian và tài nguyên.
7. **Đánh giá RAG end-to-end:** dùng inference thật được phép, khóa prompt trước eval, lưu raw output/context IDs/model/mode; chấm retrieval recall, answer correctness, citation precision/support và refusal trên bộ đủ lớn.
8. **Giữ nguyên tính minh bạch:** sample output chỉ là sample output; similarity không phải relevance; exact quote không phải entailment; điểm số cao không xóa bỏ rủi ro dữ liệu.

---

## 9. Kết luận

Chuỗi Lab 1–6 dạy một bài học quan trọng: chất lượng NLP không đến từ việc chọn kiến trúc mới nhất, mà từ việc nối đúng **dữ liệu đáng tin → baseline phù hợp → thí nghiệm công bằng → phân tích lỗi → đầu ra có thể kiểm chứng**. Lab 2 chứng minh baseline tuyến tính có thể rất mạnh; Lab 3 cho thấy mô hình chuỗi đơn giản có thể thất bại nếu cấu hình và dữ liệu đầu vào chưa phù hợp; Lab 4/5 cho thấy kiến trúc tốt hơn có thể nâng chất lượng nhưng phải trả giá bằng độ phức tạp và tài nguyên; Lab 6 mở rộng tiêu chuẩn đánh giá sang evidence, provenance và khả năng từ chối.

Kết quả đáng tin nhất là kết quả có thể truy về đúng dữ liệu, split, cấu hình và artefact. Vì vậy, bước ưu tiên tiếp theo không phải gộp mọi điểm số thành một bảng xếp hạng, mà là chuẩn hóa provenance và chạy lại các phép so sánh trên cùng giao thức.

---

## 10. Tài liệu trong workspace

- [Lab 1 README](./Lab1/README.md) · [UVN-1 audit](./Lab1/reports/vietnews_audit.md) · [transfer matrix](./Lab1/reports/transfer_matrix.md)
- [Lab 2 README](./Lab2/README.md) · [analysis report](./Lab2/reports/analysis_report.md)
- [Lab 3 README](./Lab3/README.md) · [analysis report](./Lab3/reports/analysis_report.md)
- [Lab 4 README](./Lab4/README.md) · [analysis report](./Lab4/reports/analysis_report.md)
- [Lab 5 README](./Lab5/README.md) · [analysis report](./Lab5/reports/analysis_report.md)
- [Lab 6 README](./Lab6/README.md) · [assignment](./Lab6/ASSIGNMENT.md) · [analysis report](./Lab6/reports/lab_report.md)
