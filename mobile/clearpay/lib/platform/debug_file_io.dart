import 'dart:io';

void appendDebugFile(String payload) {
  try {
    File(r'C:\Users\clt\Projects\clearpay\debug-021de0.log')
        .writeAsStringSync('$payload\n', mode: FileMode.append);
  } catch (_) {}
}
