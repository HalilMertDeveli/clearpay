import 'package:firebase_core/firebase_core.dart' show FirebaseOptions;
import 'package:flutter/foundation.dart'
    show defaultTargetPlatform, kIsWeb, TargetPlatform;

/// Stub until Halil runs `tool/configure-firebase.cmd` (FlutterFire overwrites this file).
/// Do not put a second ledger here. Wallet stays on the ASP.NET SQL host.
class DefaultFirebaseOptions {
  static FirebaseOptions get currentPlatform {
    if (kIsWeb) {
      throw UnsupportedError('ClearPay Firebase: web is not a client (Razor is).');
    }
    switch (defaultTargetPlatform) {
      case TargetPlatform.android:
      case TargetPlatform.iOS:
      case TargetPlatform.windows:
        throw UnsupportedError(
          'ClearPay Firebase: run tool/configure-firebase.cmd after firebase login.',
        );
      default:
        throw UnsupportedError(
          'ClearPay Firebase: unsupported platform $defaultTargetPlatform.',
        );
    }
  }
}
