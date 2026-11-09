import 'package:flutter/material.dart';

import 'app_strings.dart';
import 'locale_store.dart';

class LocaleScope extends InheritedWidget {
  LocaleScope({
    super.key,
    required this.store,
    required this.onChanged,
    required super.child,
  }) : code = store.code;

  final LocaleStore store;
  final VoidCallback onChanged;
  final String code;

  L get l => L(code);

  static LocaleScope? maybeOf(BuildContext context) =>
      context.dependOnInheritedWidgetOfExactType<LocaleScope>();

  static LocaleScope of(BuildContext context) {
    final scope = maybeOf(context);
    assert(scope != null, 'LocaleScope missing');
    return scope!;
  }

  @override
  bool updateShouldNotify(LocaleScope oldWidget) => code != oldWidget.code;
}

L l10n(BuildContext context) => LocaleScope.maybeOf(context)?.l ?? const L('tr');
