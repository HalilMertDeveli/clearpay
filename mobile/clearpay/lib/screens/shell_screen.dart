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

  static const _movementsIndex = 3;

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

  void _openTab(int index) {
    setState(() => _index = index);
  }

  void _selectDrawerDestination(int i) {
    Navigator.of(context).pop();
    _openTab(i);
  }

  @override
  Widget build(BuildContext context) {
    final admin = widget.api.isAdmin;
    final pages = [
      OverviewScreen(api: widget.api, onOpenTab: _openTab),
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
      appBar: AppBar(title: Text(titles[_index])),
      drawer: NavigationDrawer(
        selectedIndex: _index,
        onDestinationSelected: _selectDrawerDestination,
        children: [
          _DrawerBrand(email: widget.api.email),
          const NavigationDrawerDestination(
            icon: Icon(Icons.account_balance_wallet_outlined),
            selectedIcon: Icon(Icons.account_balance_wallet),
            label: Text('Özet'),
          ),
          const NavigationDrawerDestination(
            icon: Icon(Icons.swap_horiz),
            selectedIcon: Icon(Icons.swap_horiz),
            label: Text('Havale'),
          ),
          const NavigationDrawerDestination(
            icon: Icon(Icons.savings_outlined),
            selectedIcon: Icon(Icons.savings),
            label: Text('Yükle / Çek'),
          ),
          const NavigationDrawerDestination(
            icon: Icon(Icons.receipt_long_outlined),
            selectedIcon: Icon(Icons.receipt_long),
            label: Text('Hareketler'),
          ),
          ListTile(
            leading: const Icon(Icons.description_outlined),
            title: const Text('Dekont'),
            subtitle: const Text('Hareketler listesinden'),
            onTap: () {
              Navigator.of(context).pop();
              _openTab(_movementsIndex);
            },
          ),
          if (admin)
            const NavigationDrawerDestination(
              icon: Icon(Icons.admin_panel_settings_outlined),
              selectedIcon: Icon(Icons.admin_panel_settings),
              label: Text('Admin'),
            ),
          const Padding(
            padding: EdgeInsets.fromLTRB(28, 8, 28, 8),
            child: Divider(),
          ),
          ListTile(
            leading: const Icon(Icons.logout),
            title: const Text('Çıkış'),
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

class _DrawerBrand extends StatelessWidget {
  const _DrawerBrand({this.email});

  final String? email;

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
              const SizedBox(height: 4),
              Text(
                email ?? '',
                style: const TextStyle(color: Colors.white70, fontSize: 13),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
