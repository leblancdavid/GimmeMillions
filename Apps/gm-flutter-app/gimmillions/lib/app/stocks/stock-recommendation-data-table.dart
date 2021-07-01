import 'dart:math';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:gimmillions/app/stocks/stock-recommendation-details.dart';
import 'package:gimmillions/models/stock-recommendation-filter.dart';
import 'package:gimmillions/models/stock-recommendation.dart';

class StockRecommendationDataTableBuilder extends StatelessWidget {
  final Future<List<StockRecommendation>> _recommendations;
  final StockRecommendationFilter filter;

  const StockRecommendationDataTableBuilder(this._recommendations, this.filter);

  @override
  Widget build(BuildContext context) {
    return FutureBuilder(
        future: _recommendations,
        builder: (BuildContext context, AsyncSnapshot<List<StockRecommendation>> snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Center(child: CircularProgressIndicator(color: Theme.of(context).primaryColor));
          }

          if (snapshot.hasError) {
            return Padding(
                padding: EdgeInsets.all(16),
                child: Center(
                    child: Text(
                  'Error occurred: ${snapshot.error}',
                  style: TextStyle(color: Theme.of(context).errorColor, fontSize: 20),
                )));
          }
          if (snapshot.hasData) {
            return StockRecommendationDataTable(context, snapshot.data!, filter);
          }

          return Center(child: CircularProgressIndicator(color: Theme.of(context).primaryColor));
        });
  }
}

class StockRecommendationTableSource extends DataTableSource {
  List<StockRecommendation> _recommendations;
  BuildContext _context;
  StockRecommendationTableSource(this._recommendations, this._context);

  List<StockRecommendation> get recommendations {
    return _recommendations;
  }

  void set recommendations(List<StockRecommendation> value) {
    _recommendations = value;
    notifyListeners();
  }

  @override
  DataRow? getRow(int index) {
    var r = recommendations[index];

    return DataRow(
        cells: _getCells(r),
        onSelectChanged: (bool? selected) => {
              if (selected!)
                {
                  Navigator.pushNamed(_context, StockRecommendationDetails.routeName,
                      arguments: StockRecommendationDetailsArguments(r.symbol))
                }
            });
  }

  List<DataCell> _getCells(StockRecommendation recommendation) {
    List<DataCell> cells = [];
    cells.add(DataCell(Container(
        width: 64,
        child: Text(recommendation.symbol,
            style: TextStyle(fontWeight: FontWeight.bold, color: recommendation.getRgb(25))))));
    cells.add(DataCell(Center(
        child: Container(
            width: 64,
            child: Text(recommendation.sentiment.toStringAsFixed(2) + '%',
                style: TextStyle(fontWeight: FontWeight.bold, color: recommendation.getRgb(25)),
                textAlign: TextAlign.center)))));
    cells.add(DataCell(Center(
        child: Container(
            width: 64,
            child: Text(recommendation.confidence.toStringAsFixed(3),
                style: TextStyle(
                    fontWeight: FontWeight.bold,
                    color: recommendation.confidence > 0 ? Colors.green.shade800 : Colors.red.shade800))))));
    return cells;
  }

  void sort(int columnIndex, bool ascending) {
    if (columnIndex == 0) {
      recommendations.sort((r1, r2) => compareString(ascending, r1.symbol, r2.symbol));
    } else if (columnIndex == 1) {
      recommendations.sort((r1, r2) => compareDouble(ascending, r1.sentiment, r2.sentiment));
    } else if (columnIndex == 2) {
      recommendations.sort((r1, r2) => compareDouble(ascending, r1.confidence, r2.confidence));
    }

    notifyListeners();
  }

  int compareString(bool ascending, String value1, String value2) =>
      ascending ? value1.compareTo(value2) : value2.compareTo(value1);

  int compareDouble(bool ascending, double value1, double value2) =>
      ascending ? value1.compareTo(value2) : value2.compareTo(value1);

  @override
  bool get isRowCountApproximate => false;

  @override
  int get rowCount => recommendations.length;

  @override
  int get selectedRowCount => 0;
}

class StockRecommendationDataTable extends StatefulWidget {
  final List<StockRecommendation> recommendations;
  final StockRecommendationFilter filter;
  final BuildContext context;
  StockRecommendationDataTable(this.context, this.recommendations, this.filter);

  @override
  _StockRecommendationDataTableState createState() =>
      _StockRecommendationDataTableState(context, recommendations, filter);
}

class _StockRecommendationDataTableState extends State<StockRecommendationDataTable> {
  final List<StockRecommendation> _recommendations;
  late StockRecommendationTableSource _source;
  final StockRecommendationFilter _filter;
  BuildContext _context;
  int sortColumnIndex = 1;
  bool isAscending = false;

  _StockRecommendationDataTableState(this._context, this._recommendations, this._filter) {
    _source = StockRecommendationTableSource(_recommendations, _context);
  }

  @override
  void initState() {
    super.initState();
    _source.recommendations = _filter.filter(_recommendations);
    _filter.addListener(() {
      setState(() {
        _source.recommendations = _filter.filter(_recommendations);
      });
    });
    sortColumnIndex = 1;
    isAscending = false;
  }

  @override
  Widget build(BuildContext context) {
    return buildDataTable();
  }

  Widget buildDataTable() {
    final columns = ['Symbol', 'Sentiment', 'Confidence'];
    onSort(sortColumnIndex, isAscending);
    var rowsPerPage = min(20, _source.rowCount);
    if (rowsPerPage < 1) {
      rowsPerPage = 1;
    }
    var table = PaginatedDataTable(
      sortAscending: isAscending,
      sortColumnIndex: sortColumnIndex,
      showCheckboxColumn: false,
      rowsPerPage: rowsPerPage,
      columnSpacing: 24,
      columns: getColumns(columns),
      source: _source,
    );

    return SingleChildScrollView(padding: EdgeInsets.all(8), child: table, scrollDirection: Axis.vertical);
  }

  List<DataColumn> getColumns(List<String> columns) => columns
      .map((String column) => DataColumn(
            label: Text(column),
            onSort: onSort,
          ))
      .toList();

  void onSort(int columnIndex, bool ascending) {
    _source.sort(columnIndex, ascending);

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
