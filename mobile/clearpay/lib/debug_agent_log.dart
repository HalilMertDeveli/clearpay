import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'platform/debug_file.dart';
import 'platform/host.dart';

/// Session 021de0 ingest. Never logs tokens, passwords, or PII.
void agentDebugLog({
  required String hypothesisId,
  required String location,
  required String message,
  Map<String, Object?> data = const {},
}) {
  // #region agent log
  final payload = jsonEncode({
    'sessionId': '021de0',
    'runId': 'web-fix',
    'hypothesisId': hypothesisId,
    'location': location,
    'message': message,
    'data': data,
    'timestamp': DateTime.now().millisecondsSinceEpoch,
  });
  appendDebugFile(payload);
  final host = isAndroidHost ? '10.0.2.2' : '127.0.0.1';
  http
      .post(
        Uri.parse(
          'http://$host:7320/ingest/8265b831-5f86-4494-a083-68cbc6788d32',
        ),
        headers: {
          'Content-Type': 'application/json',
          'X-Debug-Session-Id': '021de0',
        },
        body: payload,
      )
      .ignore();
  // #endregion
}
