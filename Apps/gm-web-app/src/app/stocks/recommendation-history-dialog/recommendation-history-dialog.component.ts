import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StockData } from '../stock-data';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationHistory } from '../stock-recommendation-history';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-recommendation-history-dialog',
  templateUrl: './recommendation-history-dialog.component.html',
  styleUrls: ['./recommendation-history-dialog.component.scss']
})
export class RecommendationHistoryDialogComponent implements OnInit {

  public isLoading: boolean;
  public fontColor!: string;
  public backgroundColor!: string;

  public history: StockRecommendationHistory;
  constructor(public dialogRef: MatDialogRef<RecommendationHistoryDialogComponent>,
    private stockRecommendationService: StockRecommendationService, 
    @Inject(MAT_DIALOG_DATA) public data: {symbol: string}) { 
      this.isLoading = true;
      this.history = new StockRecommendationHistory('', data.symbol, new Date(),
        new StockRecommendation(new Date(), data.symbol, '', 50.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 
          new StockData(new Date(), data.symbol, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)),
        []);

      this.fontColor = this.history.lastRecommendation.getHsl(25);
      this.backgroundColor = this.history.lastRecommendation.getHsl(90);
    }

  ngOnInit(): void {
    this.stockRecommendationService.getHistoryFor(this.history.symbol).subscribe(x => {
      this.history = x;
      this.isLoading = false;
      this.fontColor = this.history.lastRecommendation.getHsl(25);
      this.backgroundColor = this.history.lastRecommendation.getHsl(90);
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
