import 'package:flutter/material.dart';

const navy = Color(0xFF1B2A4A);
const navyHero = Color(0xFF24365C);
const wash = Color(0xFFE8EEF5);
const teal = Color(0xFF0F766E);
const muted = Color(0xFF5C6B86);
const line = Color(0xFFD8DEE8);

final ThemeData clearPayTheme = ThemeData(
  colorScheme: ColorScheme.fromSeed(
    seedColor: navy,
    primary: navy,
    surface: Colors.white,
  ),
  scaffoldBackgroundColor: Colors.white,
  appBarTheme: const AppBarTheme(
    backgroundColor: navy,
    foregroundColor: Colors.white,
    elevation: 0,
  ),
  navigationDrawerTheme: const NavigationDrawerThemeData(
    backgroundColor: Colors.white,
    indicatorColor: Color(0x221B2A4A),
  ),
  useMaterial3: true,
);

class DemoFooter extends StatelessWidget {
  const DemoFooter({super.key});

  @override
  Widget build(BuildContext context) {
    return const Padding(
      padding: EdgeInsets.symmetric(vertical: 12),
      child: Text(
        'Demo — sahte banka gateway.',
        textAlign: TextAlign.center,
        style: TextStyle(color: navy, fontSize: 13),
      ),
    );
  }
}

String formatTry(num amount) {
  return '${amount.toStringAsFixed(2).replaceAll('.', ',')} ₺';
}
