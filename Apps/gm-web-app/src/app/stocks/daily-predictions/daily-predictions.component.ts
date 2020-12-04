import { Component, OnInit } from '@angular/core';
import { RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-daily-predictions',
  templateUrl: './daily-predictions.component.html',
  styleUrls: ['./daily-predictions.component.scss']
})
export class DailyPredictionsComponent implements OnInit {

  public predictions: RecommendationList;
  public selectedItem?: StockRecommendation;
  public isRefreshing: boolean;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.predictions = new RecommendationList();
    this.isRefreshing = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  refresh() {
    this.isRefreshing = true;
    this.predictions = new RecommendationList();
    this.stockRecommendationService.getDailyPicks().subscribe(x => {
      
      this.isRefreshing = false;
      this.predictions.recommendations = x;
      if(this.predictions.recommendations.length > 0) {
        this.selectedItem = this.predictions.recommendations[0];
      }
    }, error => {
      this.isRefreshing = false;
    });
  }

  onFilterKeyup(event: Event) {
    this.predictions.applyFilter((event.target as HTMLInputElement).value);
    if(this.predictions.sorted.length > 0) {
      this.selectedItem = this.predictions.sorted[0];
    }
  }
}
