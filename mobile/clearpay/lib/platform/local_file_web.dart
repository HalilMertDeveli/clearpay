final Map<String, String> _memory = {};

Future<String?> readLocalText(String name) async => _memory[name];

Future<void> writeLocalText(String name, String value) async {
  _memory[name] = value;
}

Future<void> deleteLocalText(String name) async {
  _memory.remove(name);
}
