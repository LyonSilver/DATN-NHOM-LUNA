MAP 2 - LOST SKY GARDEN | UNITY ASSET LAYERS

Top-level PNGs (00-04): TAT CA đều có canvas 1672 x 941 px. Đặt cùng vị trí Transform, cùng Pixels Per Unit, pivot Center để khớp chính xác.
Sắp xếp Sorting Order: 00 Background = 0, 01 Stone Bridge = 5, 02 Battle Floor = 10, 03/04 Guardians = 15.
Original_Map2_Reference.png: ảnh gốc nguyên bản để so sánh. Preview_Stacked.png: bản ghép kiểm tra.
Sprites_Cropped/: 4 PNG cắt sát giới hạn vùng có alpha, không có viền canvas thừa. Tọa độ góc trên bên trái lưu trong Unity_Layer_Positions.json.
ĐỂ IMPORT: chọn PNG trong Unity Inspector > Texture Type Sprite (2D and UI), Sprite Mode Single, Mesh Type Full Rect hoặc Tight phù hợp, Filter Mode Point nếu thích pixel-art, Compression None.
LƯU Ý: Tách từ ảnh phẳng không có dữ liệu đối tượng bị che. Background_Reconstructed được khôi phục gần đúng ở những vùng bị sàn/tượng che, không phải khung cảnh gốc chính xác. Đường cắt silhouette được xử lý bằng polygon và graph-cut; khi soi 100% vẫn có thể có vài pixel sai cạnh. Bộ này phù hợp nền tĩnh và parallax nhẹ, không thể cam kết mọi lớp tách hoàn toàn hoàn hảo.
