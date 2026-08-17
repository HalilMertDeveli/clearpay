import 'dart:io';

void appendDebugFile(String payload) {
  try {
    File(r'D:\ClearPay\clearpay\debug-021de0.log')
        .writeAsStringSync('$payload\n', mode: FileMode.append);
  } catch (_) {}
}
