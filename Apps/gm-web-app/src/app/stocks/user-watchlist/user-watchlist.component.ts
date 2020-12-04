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
  public selectedItem?: StockRecommendation;
  public isRefreshing: boolean;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.watchlist = new RecommendationList();
    this.isRefreshing = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  refresh() {
    this.isRefreshing = true;
    this.watchlist = new RecommendationList();
    this.stockRecommendationService.getFutures().subscribe(x => {
      
      this.isRefreshing = false;
      this.watchlist.recommendations = x;
      if(this.watchlist.recommendations.length > 0) {
        this.selectedItem = this.watchlist.recommendations[0];
      }
    }, error => {
      this.isRefreshing = false;
    });
  }

  onFilterKeyup(event: Event) {
    this.watchlist.applyFilter((event.target as HTMLInputElement).value);
    if(this.watchlist.sorted.length > 0) {
      this.selectedItem = this.watchlist.sorted[0];
    }
  }
}
