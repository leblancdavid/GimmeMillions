import { newArray } from '@angular/compiler/src/util';
import { Component, OnInit } from '@angular/core';
import { RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';
import { UserWatchlistService } from './user-watchlist.service';

@Component({
  selector: 'gm-user-watchlist',
  templateUrl: './user-watchlist.component.html',
  styleUrls: ['./user-watchlist.component.scss']
})
export class UserWatchlistComponent implements OnInit {

  public selectedItem?: StockRecommendation;
  public isRefreshing: boolean;
  constructor(private userWatchlistService: UserWatchlistService) {
    this.isRefreshing = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  refresh() {
    this.isRefreshing = true;
    this.userWatchlistService.refreshWatchlist().subscribe(x => {
      this.isRefreshing = false;
    });
  }

  onFilterKeyup(event: Event) {
    this.userWatchlistService.watchlist.applyFilter((event.target as HTMLInputElement).value);
    if(this.userWatchlistService.watchlist.sorted.length > 0) {
      this.selectedItem = this.userWatchlistService.watchlist.sorted[0];
    }
  }
}
