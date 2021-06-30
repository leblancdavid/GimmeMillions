import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

enum WatchState { watching, notWatching }

class WatchButton extends StatefulWidget {
  final Function? onWatch;
  final Function? onUnwatch;
  final WatchState? initialState;
  WatchButton({this.onWatch, this.onUnwatch, this.initialState});
  @override
  _WatchButtonState createState() => _WatchButtonState(onWatch, onUnwatch, this.initialState);
}

class _WatchButtonState extends State<WatchButton> {
  Function? _onWatch;
  Function? _onUnwatch;
  WatchState? _state;

  _WatchButtonState(this._onWatch, this._onUnwatch, this._state);

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton.extended(
        onPressed: () {
          if (_state == WatchState.watching) {
            _state = WatchState.notWatching;
            _onUnwatch?.call();
          } else {
            _state = WatchState.watching;
            _onWatch?.call();
          }
          setState(() {});
        },
        backgroundColor: Theme.of(context).primaryColor,
        foregroundColor: Colors.white,
        icon: Icon(Icons.remove_red_eye_outlined),
        label: Text(_state == WatchState.watching ? 'Unwatch' : 'Watch'));
  }
}
