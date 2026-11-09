import 'dart:io';

Future<String?> readLocalText(String name) async {
  final file = _file(name);
  if (await file.exists()) {
    final text = (await file.readAsString()).trim();
    return text.isEmpty ? null : text;
  }
  return null;
}

Future<void> writeLocalText(String name, String value) async {
  final file = _file(name);
  await file.parent.create(recursive: true);
  await file.writeAsString(value);
}

Future<void> deleteLocalText(String name) async {
  final file = _file(name);
  if (await file.exists()) {
    await file.delete();
  }
}

File _file(String name) {
  final root = Platform.environment['LOCALAPPDATA']
      ?? Platform.environment['HOME']
      ?? Directory.systemTemp.path;
  return File('$root${Platform.pathSeparator}ClearPay${Platform.pathSeparator}$name');
}
