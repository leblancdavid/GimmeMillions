import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationDataTable extends StatefulWidget {
  final List<StockRecommendation> recommendations;

  StockRecommendationDataTable(this.recommendations);

  @override
  _StockRecommendationDataTableState createState() => _StockRecommendationDataTableState(recommendations);
}

class _StockRecommendationDataTableState extends State<StockRecommendationDataTable> {
  final List<StockRecommendation> _recommendations;
  int? sortColumnIndex;
  bool isAscending = false;

  _StockRecommendationDataTableState(this._recommendations);

  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) => buildDataTable();

  Widget buildDataTable() {
    final columns = ['Symbol', 'Sentiment', 'Confidence'];

    return DataTable(
      sortAscending: isAscending,
      sortColumnIndex: sortColumnIndex,
      columns: getColumns(columns),
      rows: getRows(_recommendations),
    );
  }

  List<DataColumn> getColumns(List<String> columns) => columns
      .map((String column) => DataColumn(
            label: Text(column),
            onSort: onSort,
          ))
      .toList();

  List<DataRow> getRows(List<StockRecommendation> recommendations) => recommendations.map((StockRecommendation r) {
        return DataRow(cells: getCells(r));
      }).toList();

  List<DataCell> getCells(StockRecommendation recommendation) {
    List<DataCell> cells = [];
    cells.add(DataCell(
        Text(recommendation.symbol, style: TextStyle(fontWeight: FontWeight.bold, color: recommendation.getRgb(25)))));
    cells.add(DataCell(Center(
        child: Text(recommendation.sentiment.toStringAsFixed(2),
            style: TextStyle(fontWeight: FontWeight.bold, color: recommendation.getRgb(25)),
            textAlign: TextAlign.center))));
    cells.add(DataCell(Center(
        child: Text(recommendation.confidence.toStringAsFixed(3),
            style: TextStyle(
                fontWeight: FontWeight.bold,
                color: recommendation.confidence > 0 ? Colors.green.shade800 : Colors.red.shade800)))));
    return cells;
  }

  void onSort(int columnIndex, bool ascending) {
    if (columnIndex == 0) {
      _recommendations.sort((r1, r2) => compareString(ascending, r1.symbol, r2.symbol));
    } else if (columnIndex == 1) {
      _recommendations.sort((r1, r2) => compareDouble(ascending, r1.sentiment, r2.sentiment));
    } else if (columnIndex == 2) {
      _recommendations.sort((r1, r2) => compareDouble(ascending, r1.confidence, r2.confidence));
    }

    setState(() {
      this.sortColumnIndex = columnIndex;
      this.isAscending = ascending;
    });
  }

  int compareString(bool ascending, String value1, String value2) =>
      ascending ? value1.compareTo(value2) : value2.compareTo(value1);

  int compareDouble(bool ascending, double value1, double value2) =>
      ascending ? value1.compareTo(value2) : value2.compareTo(value1);
}

class ScrollableWidget {}
