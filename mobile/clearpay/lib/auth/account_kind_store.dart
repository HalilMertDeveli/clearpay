import '../platform/local_file.dart';

const bireyselKind = 'Bireysel';
const kurumsalKind = 'Kurumsal';

String normalizeAccountKind(String? raw) {
  final value = (raw ?? '').trim();
  if (value.toLowerCase() == kurumsalKind.toLowerCase()) {
    return kurumsalKind;
  }
  return bireyselKind;
}

/// Last Bireysel/Kurumsal pick. JWT file sibling — not Firebase, not a ledger.
class AccountKindStore {
  AccountKindStore();

  static const _name = 'account_kind.txt';

  String kind = bireyselKind;

  Future<void> load() async {
    final stored = await readLocalText(_name);
    if (stored != null) {
      kind = normalizeAccountKind(stored);
    }
  }

  Future<void> save(String value) async {
    kind = normalizeAccountKind(value);
    await writeLocalText(_name, kind);
  }
}

class MemoryAccountKindStore extends AccountKindStore {
  MemoryAccountKindStore([String initial = bireyselKind]) {
    kind = normalizeAccountKind(initial);
  }

  @override
  Future<void> load() async {}

  @override
  Future<void> save(String value) async {
    kind = normalizeAccountKind(value);
  }
}
