import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'gm-recommendation-history-dialog',
  templateUrl: './recommendation-history-dialog.component.html',
  styleUrls: ['./recommendation-history-dialog.component.scss']
})
export class RecommendationHistoryDialogComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: {symbol: string}) { }

  ngOnInit(): void {
  }

}
