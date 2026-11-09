import '../platform/local_file.dart';

const supportedLocales = ['tr', 'en', 'de', 'fr'];

String normalizeLocale(String? raw) {
  final value = (raw ?? '').trim().toLowerCase();
  if (supportedLocales.contains(value)) {
    return value;
  }
  return 'tr';
}

/// Same four languages as the Razor cookie `c=`. Not a 9th screen.
class LocaleStore {
  LocaleStore();

  static const _name = 'ui_locale.txt';

  String code = 'tr';

  Future<void> load() async {
    final stored = await readLocalText(_name);
    if (stored != null) {
      code = normalizeLocale(stored);
    }
  }

  Future<void> save(String value) async {
    code = normalizeLocale(value);
    await writeLocalText(_name, code);
  }
}

class MemoryLocaleStore extends LocaleStore {
  MemoryLocaleStore([String initial = 'tr']) {
    code = normalizeLocale(initial);
  }

  @override
  Future<void> load() async {}

  @override
  Future<void> save(String value) async {
    code = normalizeLocale(value);
  }
}
