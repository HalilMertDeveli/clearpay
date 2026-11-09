import 'dart:typed_data';

import 'package:share_plus/share_plus.dart';

Future<void> openReceiptPdf(String correlationId, List<int> bytes) async {
  final file = XFile.fromData(
    Uint8List.fromList(bytes),
    mimeType: 'application/pdf',
    name: 'clearpay-dekont-$correlationId.pdf',
  );
  await Share.shareXFiles(
    [file],
    text: 'ClearPay dekont (demo)',
  );
}
