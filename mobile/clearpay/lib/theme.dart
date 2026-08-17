import 'package:flutter/material.dart';

import 'l10n/app_strings.dart';
import 'l10n/locale_scope.dart';

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
    final text = l10n(context).demoFooter;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 12),
      child: Text(
        text,
        textAlign: TextAlign.center,
        style: const TextStyle(color: navy, fontSize: 13),
      ),
    );
  }
}

String formatTry(num amount, {String locale = 'tr'}) {
  return L(locale).money(amount);
}

/// Launcher/splash mark: white C + teal ring (navy comes from the parent).
class BrandMark extends StatelessWidget {
  const BrandMark({super.key, this.size = 88});

  final double size;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        border: Border.all(color: teal, width: (size * 0.055).clamp(2, 5)),
      ),
      child: Text(
        'C',
        style: TextStyle(
          color: Colors.white,
          fontSize: size * 0.52,
          fontWeight: FontWeight.w800,
          height: 1,
        ),
      ),
    );
  }
}
