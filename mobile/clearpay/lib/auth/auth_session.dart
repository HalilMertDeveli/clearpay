class AuthException implements Exception {
  AuthException(this.message, {this.code});

  final String message;
  final String? code;

  @override
  String toString() => message;
}

/// Flutter identity (Firebase Auth). Wallet JWT is still ClearPay SQL.
abstract class AuthSession {
  bool get isConfigured;

  Future<String> register({
    required String email,
    required String password,
    String? fullName,
  });

  Future<String> signIn({
    required String email,
    required String password,
  });

  Future<void> sendPasswordResetEmail(String email);
}

class DisabledAuthSession implements AuthSession {
  const DisabledAuthSession();

  static const notConfigured = 'Firebase yapılandırılmadı';

  @override
  bool get isConfigured => false;

  @override
  Future<String> register({
    required String email,
    required String password,
    String? fullName,
  }) async {
    throw AuthException(notConfigured, code: 'not-configured');
  }

  @override
  Future<String> signIn({
    required String email,
    required String password,
  }) async {
    throw AuthException(notConfigured, code: 'not-configured');
  }

  @override
  Future<void> sendPasswordResetEmail(String email) async {
    throw AuthException(notConfigured, code: 'not-configured');
  }
}
