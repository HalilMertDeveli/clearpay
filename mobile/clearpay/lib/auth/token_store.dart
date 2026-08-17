import 'dart:io';

abstract class TokenStore {
  String? get token;
  Future<void> load();
  Future<void> save(String value);
  Future<void> clear();
}

class MemoryTokenStore implements TokenStore {
  MemoryTokenStore([this.token]);

  @override
  String? token;

  @override
  Future<void> load() async {}

  @override
  Future<void> save(String value) async => token = value;

  @override
  Future<void> clear() async => token = null;
}

/// JWT only. Not a wallet database.
class FileTokenStore implements TokenStore {
  @override
  String? token;

  File get _file {
    final root = Platform.environment['LOCALAPPDATA']
        ?? Platform.environment['HOME']
        ?? Directory.systemTemp.path;
    return File('$root${Platform.pathSeparator}ClearPay${Platform.pathSeparator}jwt.txt');
  }

  @override
  Future<void> load() async {
    final file = _file;
    if (await file.exists()) {
      token = (await file.readAsString()).trim();
      if (token != null && token!.isEmpty) {
        token = null;
      }
    }
  }

  @override
  Future<void> save(String value) async {
    token = value;
    await _file.parent.create(recursive: true);
    await _file.writeAsString(value);
  }

  @override
  Future<void> clear() async {
    token = null;
    final file = _file;
    if (await file.exists()) {
      await file.delete();
    }
  }
}
