import 'dart:html' as html;

Future<void> openReceiptPdf(String correlationId, List<int> bytes) async {
  final blob = html.Blob([bytes], 'application/pdf');
  final url = html.Url.createObjectUrlFromBlob(blob);
  html.AnchorElement(href: url)
    ..setAttribute('download', 'clearpay-dekont-$correlationId.pdf')
    ..click();
  html.Url.revokeObjectUrl(url);
}
