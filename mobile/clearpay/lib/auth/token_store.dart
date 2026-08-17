import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../debug_agent_log.dart';
import '../platform/host.dart';
import '../platform/local_file.dart';

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

/// JWT only. Not a wallet database. Prefers OS keystore; file is a Windows fallback.
class SecureTokenStore implements TokenStore {
  SecureTokenStore({FlutterSecureStorage? secure, FileTokenStore? file})
      : _secure = secure ?? const FlutterSecureStorage(),
        _file = file ?? FileTokenStore();

  static const _key = 'clearpay.jwt';

  final FlutterSecureStorage _secure;
  final FileTokenStore _file;

  @override
  String? token;

  @override
  Future<void> load() async {
    var source = 'empty';
    try {
      token = await _secure.read(key: _key);
      if (token != null && token!.isEmpty) {
        token = null;
      }
      if (token != null) {
        source = 'secure';
      }
    } catch (e) {
      source = 'secure_error:${e.runtimeType}';
    }

    if (token == null) {
      await _file.load();
      token = _file.token;
      if (token != null) {
        source = 'file';
        try {
          await _secure.write(key: _key, value: token!);
          await _file.clear();
          source = 'file_migrated';
        } catch (_) {
          // Keep file copy if keystore is unavailable.
        }
      }
    }
    // #region agent log
    agentDebugLog(
      hypothesisId: 'C',
      location: 'token_store.dart:load',
      message: 'token loaded',
      data: {'source': source, 'hasToken': token != null, 'os': operatingSystemName},
    );
    // #endregion
  }

  @override
  Future<void> save(String value) async {
    token = value;
    try {
      await _secure.write(key: _key, value: value);
      await _file.clear();
    } catch (_) {
      await _file.save(value);
    }
  }

  @override
  Future<void> clear() async {
    token = null;
    try {
      await _secure.delete(key: _key);
    } catch (_) {}
    await _file.clear();
  }
}

/// Legacy plaintext JWT. Used only when secure storage cannot open.
class FileTokenStore implements TokenStore {
  static const _name = 'jwt.txt';

  @override
  String? token;

  @override
  Future<void> load() async {
    token = await readLocalText(_name);
  }

  @override
  Future<void> save(String value) async {
    token = value;
    await writeLocalText(_name, value);
  }

  @override
  Future<void> clear() async {
    token = null;
    await deleteLocalText(_name);
  }
}
