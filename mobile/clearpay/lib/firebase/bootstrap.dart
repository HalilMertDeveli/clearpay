import 'package:clearpay/firebase_options.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/foundation.dart';

import '../debug_agent_log.dart';

/// Non-money health doc. Not the cash register (ledger stays SQL).
const String kClearPayFirestorePingPath = 'app_meta/ping';

const String kClearPayFirestorePingMessage = 'ClearPay ping';

enum ClearPayFirestorePingKind { skipped, wrote, failed }

/// Last ping outcome for existing login chrome (not a 9th screen).
class ClearPayFirestorePing {
  static ClearPayFirestorePingKind kind = ClearPayFirestorePingKind.skipped;
  static String? detail;
}

/// Payload must never include amounts, balances, transfers, or passwords.
Map<String, Object?> clearPayFirestorePingPayload() => {
      'ok': true,
      'client': 'flutter',
      'message': kClearPayFirestorePingMessage,
    };

/// Initializes Firebase when FlutterFire options exist. Missing config must not
/// block JWT login — the ledger is still SQL Server.
Future<void> initClearPayFirebase() async {
  // #region agent log
  agentDebugLog(
    hypothesisId: 'A',
    location: 'bootstrap.dart:init',
    message: 'firebase before',
    data: {
      'platform': defaultTargetPlatform.name,
      'kIsWeb': kIsWeb,
    },
  );
  // #endregion
  try {
    await Firebase.initializeApp(
      options: DefaultFirebaseOptions.currentPlatform,
    );
    // #region agent log
    agentDebugLog(
      hypothesisId: 'A',
      location: 'bootstrap.dart:init',
      message: 'firebase ok',
      data: {'apps': Firebase.apps.length},
    );
    // #endregion
  } catch (e, st) {
    // #region agent log
    agentDebugLog(
      hypothesisId: 'A',
      location: 'bootstrap.dart:init',
      message: 'firebase catch',
      data: {'errorType': e.runtimeType.toString()},
    );
    // #endregion
    debugPrint('ClearPay Firebase skipped (JWT client continues): $e\n$st');
  }
  await pingClearPayFirestore();
}

/// Optional Firestore ping after a successful Firebase init. Skipped when
/// Firebase was skipped. Permission/network errors must not block JWT login.
Future<void> pingClearPayFirestore() async {
  if (Firebase.apps.isEmpty) {
    ClearPayFirestorePing.kind = ClearPayFirestorePingKind.skipped;
    ClearPayFirestorePing.detail = null;
    return;
  }
  try {
    final doc = FirebaseFirestore.instance.doc(kClearPayFirestorePingPath);
    await doc
        .set({
          ...clearPayFirestorePingPayload(),
          'touchedAt': FieldValue.serverTimestamp(),
        })
        .timeout(const Duration(seconds: 8));
    final snap = await doc.get().timeout(const Duration(seconds: 8));
    final data = snap.data();
    final echoed = data?['message']?.toString();
    ClearPayFirestorePing.kind = ClearPayFirestorePingKind.wrote;
    ClearPayFirestorePing.detail = echoed ?? kClearPayFirestorePingMessage;
    // #region agent log
    agentDebugLog(
      hypothesisId: 'A',
      location: 'bootstrap.dart:ping',
      message: 'firestore ping ok',
      data: {
        'path': kClearPayFirestorePingPath,
        'exists': snap.exists,
        'message': echoed,
      },
    );
    // #endregion
  } catch (e, st) {
    ClearPayFirestorePing.kind = ClearPayFirestorePingKind.failed;
    ClearPayFirestorePing.detail = e.runtimeType.toString();
    // #region agent log
    agentDebugLog(
      hypothesisId: 'A',
      location: 'bootstrap.dart:ping',
      message: 'firestore ping skip',
      data: {'errorType': e.runtimeType.toString()},
    );
    // #endregion
    debugPrint('ClearPay Firestore ping skipped (JWT client continues): $e\n$st');
  }
}