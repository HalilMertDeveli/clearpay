import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../api/wallet_live_hub.dart';
import '../auth/account_kind_store.dart';
import '../auth/auth_session.dart';
import '../auth/token_store.dart';
import '../l10n/language_strip.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';
import 'admin_screen.dart';
import 'cards_screen.dart';
import 'funding_screen.dart';
import 'login_screen.dart';
import 'movements_screen.dart';
import 'overview_screen.dart';
import 'transfer_screen.dart';

class ShellScreen extends StatefulWidget {
  const ShellScreen({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
    this.auth = const DisabledAuthSession(),
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;
  final AuthSession auth;

  @override
  State<ShellScreen> createState() => _ShellScreenState();
}

class _ShellScreenState extends State<ShellScreen> {
  int _index = 0;
  int _liveTick = 0;
  final WalletLiveHub _live = WalletLiveHub();
  TransferPrefill? _prefill;
  int _prefillNonce = 0;
  String? _fundingAccount;
  int _fundingNonce = 0;
  var _leaving = false;

  static const _fundingIndex = 3;
  static const _movementsIndex = 4;

  String get _kind =>
      normalizeAccountKind(widget.api.accountKind ?? widget.kindStore.kind);

  @override
  void initState() {
    super.initState();
    _live.connect(
      baseUrl: widget.api.baseUrl,
      token: () => widget.store.token,
      onChanged: () {
        if (mounted) {
          setState(() => _liveTick++);
        }
      },
    );
    widget.api.onUnauthorized = _logout;
  }

  @override
  void dispose() {
    _live.dispose();
    super.dispose();
  }

  Future<void> _logout() async {
    if (_leaving) {
      return;
    }
    _leaving = true;
    widget.api.onUnauthorized = null;
    await _live.dispose();
    await widget.store.clear();
    if (!mounted) {
      return;
    }
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(
        builder: (_) => LoginScreen(
          store: widget.store,
          api: widget.api,
          kindStore: widget.kindStore,
          auth: widget.auth,
        ),
      ),
      (_) => false,
    );
  }

  void _openTab(int index) {
    setState(() => _index = index);
  }

  void _openPay(TransferPrefill prefill) {
    setState(() {
      _prefill = prefill;
      _prefillNonce++;
      _index = 1;
    });
  }

  void _openFundingFromCard(String accountHint) {
    setState(() {
      _fundingAccount = accountHint;
      _fundingNonce++;
      _index = _fundingIndex;
    });
  }

  void _selectDrawerDestination(int i) {
    Navigator.of(context).pop();
    _openTab(i);
  }

  @override
  Widget build(BuildContext context) {
    final admin = widget.api.isAdmin;
    final l = l10n(context);
    final pages = [
      OverviewScreen(
        api: widget.api,
        kind: _kind,
        onOpenTab: _openTab,
        onPayQr: _openPay,
        liveTick: _liveTick,
      ),
      TransferScreen(
        key: ValueKey('transfer-$_prefillNonce'),
        api: widget.api,
        prefill: _prefill,
      ),
      CardsScreen(
        api: widget.api,
        liveTick: _liveTick,
        onLoadFromCard: _openFundingFromCard,
      ),
      FundingScreen(
        key: ValueKey('fund-$_fundingNonce'),
        api: widget.api,
        liveTick: _liveTick,
        initialAccount: _fundingAccount,
      ),
      MovementsScreen(api: widget.api, liveTick: _liveTick),
      if (admin) AdminScreen(api: widget.api, liveTick: _liveTick),
    ];
    final titles = [
      l.overview,
      l.transfer,
      l.cards,
      l.topUpWithdraw,
      l.movements,
      if (admin) l.admin,
    ];
    return Scaffold(
      appBar: AppBar(
        title: Text(titles[_index]),
        actions: [
          const Padding(
            padding: EdgeInsets.only(right: 4),
            child: LanguageStrip(light: true),
          ),
          Center(
            child: Padding(
              padding: const EdgeInsets.only(right: 12),
              child: Text(
                _kind,
                style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600),
              ),
            ),
          ),
        ],
      ),
      drawer: NavigationDrawer(
        selectedIndex: _index,
        onDestinationSelected: _selectDrawerDestination,
        children: [
          _DrawerBrand(email: widget.api.email, kind: _kind),
          NavigationDrawerDestination(
            icon: const Icon(Icons.account_balance_wallet_outlined),
            selectedIcon: const Icon(Icons.account_balance_wallet),
            label: Text(l.overview),
          ),
          NavigationDrawerDestination(
            icon: const Icon(Icons.swap_horiz),
            selectedIcon: const Icon(Icons.swap_horiz),
            label: Text(l.transfer),
          ),
          NavigationDrawerDestination(
            icon: const Icon(Icons.credit_card_outlined),
            selectedIcon: const Icon(Icons.credit_card),
            label: Text(l.cards),
          ),
          NavigationDrawerDestination(
            icon: const Icon(Icons.savings_outlined),
            selectedIcon: const Icon(Icons.savings),
            label: Text(l.topUpWithdraw),
          ),
          NavigationDrawerDestination(
            icon: const Icon(Icons.receipt_long_outlined),
            selectedIcon: const Icon(Icons.receipt_long),
            label: Text(l.movements),
          ),
          ListTile(
            leading: const Icon(Icons.description_outlined),
            title: Text(l.receipt),
            subtitle: Text(l.receiptFromList),
            onTap: () {
              Navigator.of(context).pop();
              _openTab(_movementsIndex);
            },
          ),
          if (admin)
            NavigationDrawerDestination(
              icon: const Icon(Icons.admin_panel_settings_outlined),
              selectedIcon: const Icon(Icons.admin_panel_settings),
              label: Text(l.admin),
            ),
          const Padding(
            padding: EdgeInsets.fromLTRB(28, 8, 28, 8),
            child: Divider(),
          ),
          ListTile(
            leading: const Icon(Icons.logout),
            title: Text(l.signOut),
            onTap: () {
              Navigator.of(context).pop();
              _logout();
            },
          ),
        ],
      ),
      body: Column(
        children: [
          Expanded(child: pages[_index]),
          const DemoFooter(),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: _openTab,
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.account_balance_wallet_outlined),
            label: l.overview,
          ),
          NavigationDestination(icon: const Icon(Icons.swap_horiz), label: l.transfer),
          NavigationDestination(icon: const Icon(Icons.credit_card_outlined), label: l.cards),
          NavigationDestination(icon: const Icon(Icons.savings_outlined), label: l.topUp),
          NavigationDestination(icon: const Icon(Icons.receipt_long_outlined), label: l.movementShort),
          if (admin)
            NavigationDestination(
              icon: const Icon(Icons.admin_panel_settings_outlined),
              label: l.admin,
            ),
        ],
      ),
    );
  }
}

class _DrawerBrand extends StatelessWidget {
  const _DrawerBrand({this.email, required this.kind});

  final String? email;
  final String kind;

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: navy,
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'ClearPay',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.8,
                ),
              ),
              const LanguageStrip(light: true),
              const SizedBox(height: 8),
              Text(
                email ?? '',
                style: const TextStyle(color: Colors.white70, fontSize: 13),
              ),
              const SizedBox(height: 6),
              Text(
                '$kind · ${l10n(context).notMerchant}',
                style: const TextStyle(color: Colors.white70, fontSize: 12),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
