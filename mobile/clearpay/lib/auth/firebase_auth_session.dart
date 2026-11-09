import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_core/firebase_core.dart';

import 'auth_session.dart';

class FirebaseAuthSession implements AuthSession {
  FirebaseAuthSession({FirebaseAuth? auth}) : _auth = auth ?? FirebaseAuth.instance;

  final FirebaseAuth _auth;

  @override
  bool get isConfigured => Firebase.apps.isNotEmpty;

  @override
  Future<String> register({
    required String email,
    required String password,
    String? fullName,
  }) async {
    _ensure();
    try {
      final cred = await _auth.createUserWithEmailAndPassword(
        email: email.trim(),
        password: password,
      );
      final user = cred.user;
      if (user == null) {
        throw AuthException('Firebase kayıt tamamlanamadı.');
      }
      if (fullName != null && fullName.trim().isNotEmpty) {
        await user.updateDisplayName(fullName.trim());
      }
      return await _idToken(user);
    } on FirebaseAuthException catch (e) {
      throw AuthException(_tr(e), code: e.code);
    }
  }

  @override
  Future<String> signIn({
    required String email,
    required String password,
  }) async {
    _ensure();
    try {
      final cred = await _auth.signInWithEmailAndPassword(
        email: email.trim(),
        password: password,
      );
      final user = cred.user;
      if (user == null) {
        throw AuthException('Firebase giriş tamamlanamadı.');
      }
      return await _idToken(user);
    } on FirebaseAuthException catch (e) {
      throw AuthException(_tr(e), code: e.code);
    }
  }

  @override
  Future<void> sendPasswordResetEmail(String email) async {
    _ensure();
    try {
      await _auth.sendPasswordResetEmail(email: email.trim());
    } on FirebaseAuthException catch (e) {
      throw AuthException(_tr(e), code: e.code);
    }
  }

  void _ensure() {
    if (!isConfigured) {
      throw AuthException(DisabledAuthSession.notConfigured, code: 'not-configured');
    }
  }

  Future<String> _idToken(User user) async {
    final token = await user.getIdToken();
    if (token == null || token.isEmpty) {
      throw AuthException('Firebase kimlik jetonu alınamadı.');
    }
    return token;
  }

  static String _tr(FirebaseAuthException e) {
    switch (e.code) {
      case 'email-already-in-use':
        return 'Bu e-posta zaten kayıtlı.';
      case 'weak-password':
        return 'Şifre en az 6 karakter olmalı.';
      case 'invalid-email':
        return 'Geçerli bir e-posta girin.';
      case 'user-not-found':
      case 'wrong-password':
      case 'invalid-credential':
        return 'E-posta veya şifre hatalı.';
      case 'network-request-failed':
        return 'Ağ hatası. Firebase’e ulaşılamadı.';
      default:
        return e.message ?? 'Firebase hatası (${e.code}).';
    }
  }
}
