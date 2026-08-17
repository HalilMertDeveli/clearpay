import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/foundation.dart';

import 'firebase_options.dart';

/// Initializes Firebase when FlutterFire options exist. Missing config must not
/// block JWT login — the ledger is still SQL Server.
Future<void> initClearPayFirebase() async {
  try {
    await Firebase.initializeApp(
      options: DefaultFirebaseOptions.currentPlatform,
    );
  } catch (e, st) {
    debugPrint('ClearPay Firebase skipped (JWT client continues): $e\n$st');
  }
}
