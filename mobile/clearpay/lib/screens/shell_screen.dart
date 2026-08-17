import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/token_store.dart';
import '../theme.dart';
import 'admin_screen.dart';
import 'funding_screen.dart';
import 'login_screen.dart';
import 'movements_screen.dart';
import 'overview_screen.dart';
import 'transfer_screen.dart';

class ShellScreen extends StatefulWidget {
  const ShellScreen({super.key, required this.store, required this.api});

  final TokenStore store;
  final ClearPayClient api;

  @override
  State<ShellScreen> createState() => _ShellScreenState();
}

class _ShellScreenState extends State<ShellScreen> {
  int _index = 0;

  Future<void> _logout() async {
    await widget.store.clear();
    if (!mounted) {
      return;
    }
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(
        builder: (_) => LoginScreen(store: widget.store, api: widget.api),
      ),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final admin = widget.api.isAdmin;
    final pages = [
      OverviewScreen(api: widget.api, onOpenTab: (i) => setState(() => _index = i)),
      TransferScreen(api: widget.api),
      FundingScreen(api: widget.api),
      MovementsScreen(api: widget.api),
      if (admin) AdminScreen(api: widget.api),
    ];
    final titles = [
      'Özet',
      'Havale',
      'Yükle / Çek',
      'Hareketler',
      if (admin) 'Admin',
    ];
    return Scaffold(
      appBar: AppBar(
        title: Text(titles[_index]),
        actions: [
          IconButton(
            onPressed: _logout,
            icon: const Icon(Icons.logout),
            tooltip: 'Çıkış',
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
        onDestinationSelected: (i) => setState(() => _index = i),
        destinations: [
          const NavigationDestination(
            icon: Icon(Icons.account_balance_wallet_outlined),
            label: 'Özet',
          ),
          const NavigationDestination(icon: Icon(Icons.swap_horiz), label: 'Havale'),
          const NavigationDestination(icon: Icon(Icons.savings_outlined), label: 'Yükle'),
          const NavigationDestination(icon: Icon(Icons.receipt_long_outlined), label: 'Hareket'),
          if (admin)
            const NavigationDestination(icon: Icon(Icons.admin_panel_settings_outlined), label: 'Admin'),
        ],
      ),
    );
  }
}
