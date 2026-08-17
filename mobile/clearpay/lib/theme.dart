import 'package:flutter/material.dart';

const navy = Color(0xFF1B2A4A);

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
