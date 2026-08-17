import 'package:signalr_netcore/http_connection_options.dart';
import 'package:signalr_netcore/hub_connection.dart';
import 'package:signalr_netcore/hub_connection_builder.dart';

import '../debug_agent_log.dart';

/// T-071: refresh hint from `/hubs/wallet`. Does not store a balance.
class WalletLiveHub {
  HubConnection? _connection;

  Future<void> connect({
    required String baseUrl,
    required String? Function() token,
    required void Function() onChanged,
  }) async {
    await dispose();
    final root = baseUrl.endsWith('/') ? baseUrl.substring(0, baseUrl.length - 1) : baseUrl;
    final options = HttpConnectionOptions(
      accessTokenFactory: () async => token() ?? '',
    );
    final connection = HubConnectionBuilder()
        .withUrl('$root/hubs/wallet', options: options)
        .withAutomaticReconnect()
        .build();
    connection.on('WalletChanged', (_) {
      onChanged();
    });
    _connection = connection;
    var ok = false;
    String? errorType;
    try {
      await connection.start();
      ok = true;
    } catch (e) {
      errorType = e.runtimeType.toString();
      await dispose();
    }
    // #region agent log
    agentDebugLog(
      hypothesisId: 'D',
      location: 'wallet_live_hub.dart:connect',
      message: 'hub connect',
      data: {'ok': ok, 'errorType': errorType, 'host': Uri.tryParse(root)?.host},
    );
    // #endregion
  }

  Future<void> dispose() async {
    final connection = _connection;
    _connection = null;
    if (connection == null) {
      return;
    }
    try {
      await connection.stop();
    } catch (_) {
      /* already down */
    }
  }
}
