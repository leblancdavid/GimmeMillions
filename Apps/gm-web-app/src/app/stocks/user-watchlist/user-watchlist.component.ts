import { newArray } from '@angular/compiler/src/util';
import { Component, OnInit } from '@angular/core';
import { RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-user-watchlist',
  templateUrl: './user-watchlist.component.html',
  styleUrls: ['./user-watchlist.component.scss']
})
export class UserWatchlistComponent implements OnInit {

  public watchlist: RecommendationList;
  public selectedFuture?: StockRecommendation;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.watchlist = new RecommendationList();
   }

  ngOnInit(): void {
    this.stockRecommendationService.getFutures().subscribe(x => {
      this.watchlist.recommendations = x;
      if(this.watchlist.recommendations.length > 0) {
        this.selectedFuture = this.watchlist.recommendations[0];
      }
    })
  }

  onFilterKeyup(event: Event) {
    this.watchlist.applyFilter((event.target as HTMLInputElement).value);
  }

  selectFuture(r: StockRecommendation) {
    if(this.selectedFuture && r.symbol == this.selectedFuture.symbol) {
      this.selectedFuture = undefined;
    } else {
      this.selectedFuture = r;
    }
  }
}
