import 'dart:io';

import 'package:share_plus/share_plus.dart';

Future<void> openReceiptPdf(String correlationId, List<int> bytes) async {
  final root = Platform.environment['LOCALAPPDATA'] ??
      Platform.environment['HOME'] ??
      Directory.systemTemp.path;
  final dir = Directory('$root${Platform.pathSeparator}ClearPay${Platform.pathSeparator}receipts');
  await dir.create(recursive: true);
  final file = File('${dir.path}${Platform.pathSeparator}clearpay-dekont-$correlationId.pdf');
  await file.writeAsBytes(bytes, flush: true);

  if (Platform.isAndroid || Platform.isIOS) {
    await Share.shareXFiles(
      [XFile(file.path)],
      text: 'ClearPay dekont (demo)',
    );
    return;
  }

  if (Platform.isWindows) {
    await Process.start('cmd', ['/c', 'start', '', file.path]);
    return;
  }
  if (Platform.isMacOS) {
    await Process.start('open', [file.path]);
    return;
  }
  await Process.start('xdg-open', [file.path]);
}
