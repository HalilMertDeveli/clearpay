import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../api/clearpay_client.dart';
import '../auth/account_kind_store.dart';
import '../debug_agent_log.dart';
import '../l10n/locale_scope.dart';
import '../qr/pay_uri.dart';
import '../theme.dart';
import 'receipt_screen.dart';
import 'transfer_screen.dart';

class OverviewScreen extends StatefulWidget {
  const OverviewScreen({
    super.key,
    required this.api,
    required this.kind,
    this.onOpenTab,
    this.onPayQr,
    this.liveTick = 0,
  });

  final ClearPayClient api;
  final String kind;
  final ValueChanged<int>? onOpenTab;
  final ValueChanged<TransferPrefill>? onPayQr;
  final int liveTick;

  @override
  State<OverviewScreen> createState() => _OverviewScreenState();
}

class _OverviewScreenState extends State<OverviewScreen> {
  WalletSnapshot? _wallet;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void didUpdateWidget(OverviewScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.liveTick != widget.liveTick) {
      _load();
    }
  }

  Future<void> _load() async {
    try {
      final wallet = await widget.api.wallet();
      if (!mounted) {
        return;
      }
      setState(() {
        _wallet = wallet;
        _error = null;
      });
      // #region agent log
      agentDebugLog(
        hypothesisId: 'C',
        location: 'overview_screen.dart:_load',
        message: 'wallet load ok',
        data: {
          'frozen': wallet.isFrozen,
          'movements': wallet.lastMovements.length,
        },
      );
      // #endregion
    } on ApiException catch (e) {
      // #region agent log
      agentDebugLog(
        hypothesisId: 'M',
        location: 'overview_screen.dart:_load',
        message: 'wallet load failed',
        data: {'status': e.status},
      );
      // #endregion
      if (!mounted) {
        return;
      }
      setState(() => _error = e.message);
    } catch (e) {
      // #region agent log
      agentDebugLog(
        hypothesisId: 'C',
        location: 'overview_screen.dart:_load',
        message: 'wallet load other',
        data: {'errorType': e.runtimeType.toString()},
      );
      // #endregion
      rethrow;
    }
  }

  void _openReceipt(String correlationId) {
    if (correlationId.isEmpty) {
      return;
    }
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => ReceiptScreen(api: widget.api, correlationId: correlationId),
      ),
    );
  }

  void _showPark(String title, String body) {
    final l = l10n(context);
    showModalBottomSheet<void>(
      context: context,
      builder: (ctx) => Padding(
        padding: const EdgeInsets.fromLTRB(24, 20, 24, 32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: const TextStyle(color: navy, fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text(body, style: const TextStyle(color: muted)),
            const SizedBox(height: 12),
            Text(l.parkNotDemo, style: const TextStyle(color: navy, fontWeight: FontWeight.w600)),
            const DemoFooter(),
          ],
        ),
      ),
    );
  }

  void _showReceiveQr() {
    final l = l10n(context);
    final email = widget.api.email;
    if (email == null || email.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l.qrNeedsEmail)),
      );
      return;
    }
    final payload = PayUri(to: email).encode();
    showModalBottomSheet<void>(
      context: context,
      builder: (ctx) => Padding(
        padding: const EdgeInsets.fromLTRB(24, 20, 24, 32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(l.receiveQrTitle, style: const TextStyle(color: navy, fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text(
              l.receiveQrLede,
              textAlign: TextAlign.center,
              style: const TextStyle(color: muted, fontSize: 13),
            ),
            const SizedBox(height: 16),
            ColoredBox(color: Colors.white, child: QrImageView(data: payload, size: 200)),
            const SizedBox(height: 12),
            SelectableText(payload, style: const TextStyle(fontSize: 12, color: navy)),
            TextButton(
              onPressed: () async {
                await Clipboard.setData(ClipboardData(text: payload));
                if (ctx.mounted) {
                  Navigator.pop(ctx);
                }
              },
              child: Text(l.copyUri),
            ),
          ],
        ),
      ),
    );
  }

  void _showPayQr() {
    final l = l10n(context);
    final payload = TextEditingController();
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 20, 24, 24 + MediaQuery.viewInsetsOf(ctx).bottom),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(l.payQrTitle, style: const TextStyle(color: navy, fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text(
              l.payQrLede,
              style: const TextStyle(color: muted, fontSize: 13),
            ),
            TextField(
              controller: payload,
              decoration: InputDecoration(labelText: l.qrPayload),
            ),
            const SizedBox(height: 12),
            FilledButton(
              onPressed: () {
                final parsed = PayUri.tryParse(payload.text);
                if (parsed == null) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text(l.invalidQr)),
                  );
                  return;
                }
                Navigator.pop(ctx);
                widget.onPayQr?.call(TransferPrefill(recipient: parsed.to, amount: parsed.amount));
              },
              child: Text(l.fillTransferForm),
            ),
            const DemoFooter(),
          ],
        ),
      ),
    );
  }

  void _showMore() {
    final l = l10n(context);
    showModalBottomSheet<void>(
      context: context,
      builder: (ctx) => ListView(
        shrinkWrap: true,
        padding: const EdgeInsets.fromLTRB(8, 16, 8, 24),
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 8),
            child: Text(l.more, style: const TextStyle(color: navy, fontSize: 18, fontWeight: FontWeight.w700)),
          ),
          ListTile(
            leading: const Icon(Icons.account_balance_wallet_outlined),
            title: Text(l.overview),
            onTap: () {
              Navigator.pop(ctx);
              widget.onOpenTab?.call(0);
            },
          ),
          ListTile(
            leading: const Icon(Icons.swap_horiz),
            title: Text(l.transfer),
            onTap: () {
              Navigator.pop(ctx);
              widget.onOpenTab?.call(1);
            },
          ),
          ListTile(
            leading: const Icon(Icons.savings_outlined),
            title: Text(l.topUpWithdraw),
            onTap: () {
              Navigator.pop(ctx);
              widget.onOpenTab?.call(2);
            },
          ),
          ListTile(
            leading: const Icon(Icons.receipt_long_outlined),
            title: Text(l.movementsReceipt),
            onTap: () {
              Navigator.pop(ctx);
              widget.onOpenTab?.call(3);
            },
          ),
          if (widget.api.isAdmin)
            ListTile(
              leading: const Icon(Icons.admin_panel_settings_outlined),
              title: Text(l.admin),
              onTap: () {
                Navigator.pop(ctx);
                widget.onOpenTab?.call(4);
              },
            ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.show_chart),
            title: Text(l.markets),
            subtitle: Text(l.parkNotDemo),
            onTap: () {
              Navigator.pop(ctx);
              _showPark(l.markets, l.marketsPark);
            },
          ),
          ListTile(
            leading: const Icon(Icons.receipt_outlined),
            title: Text(l.invoice),
            subtitle: Text(l.parkNotDemo),
          ),
          ListTile(
            leading: const Icon(Icons.credit_score_outlined),
            title: Text(l.credit),
            subtitle: Text(l.parkNotDemo),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }

  void _openFast() {
    final l = l10n(context);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(l.fastSnack)),
    );
    widget.onOpenTab?.call(1);
  }

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    final kurumsal = widget.kind == kurumsalKind;
    return ColoredBox(
      color: wash,
      child: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
          children: [
            if (_error != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(_error!, style: const TextStyle(color: Colors.red)),
              ),
            if (_wallet == null && _error == null)
              const Padding(
                padding: EdgeInsets.only(top: 48),
                child: Center(child: CircularProgressIndicator()),
              ),
            if (_wallet != null) ...[
              _BalanceCard(wallet: _wallet!, kind: widget.kind),
              const SizedBox(height: 16),
              Text(
                kurumsal ? l.corporateShortcuts : l.quickOps,
                style: const TextStyle(fontWeight: FontWeight.w600, color: navy),
              ),
              const SizedBox(height: 8),
              GridView.count(
                crossAxisCount: 4,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                mainAxisSpacing: 8,
                crossAxisSpacing: 8,
                childAspectRatio: 0.82,
                children: [
                  _QuickTile(
                    icon: Icons.swap_horiz,
                    label: l.transfer,
                    enabled: !_wallet!.isFrozen,
                    onTap: () => widget.onOpenTab?.call(1),
                  ),
                  _QuickTile(
                    icon: Icons.add_card_outlined,
                    label: l.topUp,
                    enabled: !_wallet!.isFrozen,
                    onTap: () => widget.onOpenTab?.call(2),
                  ),
                  _QuickTile(
                    icon: Icons.south_west,
                    label: l.withdraw,
                    enabled: !_wallet!.isFrozen,
                    onTap: () => widget.onOpenTab?.call(2),
                  ),
                  _QuickTile(icon: Icons.qr_code_2, label: l.qrReceive, onTap: _showReceiveQr),
                  _QuickTile(
                    icon: Icons.qr_code_scanner,
                    label: l.qrPay,
                    enabled: !_wallet!.isFrozen,
                    onTap: _showPayQr,
                  ),
                  _QuickTile(
                    icon: Icons.bolt_outlined,
                    label: l.fast,
                    enabled: !_wallet!.isFrozen,
                    onTap: _openFast,
                  ),
                  _QuickTile(
                    icon: Icons.show_chart,
                    label: l.markets,
                    onTap: () => _showPark(l.markets, l.marketsPark),
                  ),
                  _QuickTile(icon: Icons.more_horiz, label: l.more, onTap: _showMore),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                l.fastHint,
                style: const TextStyle(color: muted, fontSize: 12),
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      l.recentActivity,
                      style: const TextStyle(fontWeight: FontWeight.w600, color: navy),
                    ),
                  ),
                  TextButton(
                    onPressed: () => widget.onOpenTab?.call(3),
                    child: Text(l.viewAll),
                  ),
                ],
              ),
              if (_wallet!.lastMovements.isEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: 8),
                  child: Text(l.noMovementsYet, style: const TextStyle(color: muted)),
                ),
              for (final row in _wallet!.lastMovements)
                Material(
                  color: Colors.white,
                  child: ListTile(
                    title: Text(row.kind),
                    subtitle: Text(row.at, style: const TextStyle(color: muted)),
                    trailing: Text(
                      l.money(row.amount),
                      style: const TextStyle(fontWeight: FontWeight.w600, color: navy),
                    ),
                    onTap: () => _openReceipt(row.correlationId),
                  ),
                ),
            ],
          ],
        ),
      ),
    );
  }
}

class _BalanceCard extends StatelessWidget {
  const _BalanceCard({required this.wallet, required this.kind});

  final WalletSnapshot wallet;
  final String kind;

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 20, 20, 18),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [navyHero, navy],
        ),
        borderRadius: BorderRadius.all(Radius.circular(12)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            kind == kurumsalKind ? l.corporateWallet : l.wallet,
            style: const TextStyle(color: Colors.white70, fontSize: 13, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          Text(
            l.money(wallet.balance),
            style: const TextStyle(
              color: Colors.white,
              fontSize: 32,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            l.monthFlow(l.money(wallet.monthOutgoing), l.money(wallet.monthIncoming)),
            style: const TextStyle(color: Colors.white70, fontSize: 13),
          ),
          if (wallet.isFrozen) ...[
            const SizedBox(height: 10),
            Text(
              l.walletFrozen,
              style: const TextStyle(color: Color(0xFFFFC9C9), fontWeight: FontWeight.w600),
            ),
          ],
        ],
      ),
    );
  }
}

class _QuickTile extends StatelessWidget {
  const _QuickTile({
    required this.icon,
    required this.label,
    required this.onTap,
    this.enabled = true,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;
  final bool enabled;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: enabled ? Colors.white : const Color(0xFFF0F3F8),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(12)),
        side: BorderSide(color: line),
      ),
      child: InkWell(
        onTap: enabled ? onTap : null,
        borderRadius: const BorderRadius.all(Radius.circular(12)),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 4),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, color: enabled ? teal : muted),
              const SizedBox(height: 6),
              FittedBox(
                fit: BoxFit.scaleDown,
                child: Text(
                  label,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 11,
                    color: enabled ? navy : muted,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

