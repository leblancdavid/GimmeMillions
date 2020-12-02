import { Component, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-user-watchlist',
  templateUrl: './user-watchlist.component.html',
  styleUrls: ['./user-watchlist.component.scss']
})
export class UserWatchlistComponent implements OnInit {

  public watchlist: Array<StockRecommendation>;
  public selectedItem?: StockRecommendation;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.watchlist = new Array<StockRecommendation>();
   }

  ngOnInit(): void {
    this.stockRecommendationService.getFutures().subscribe(x => {
      for(let r of x) {
        this.watchlist.push(r);
      }
      if(this.watchlist.length > 0) {
        this.selectedItem = this.watchlist[0];
      }
    })
  }

  select(r: StockRecommendation) {
    if(this.selectedItem && r.symbol == this.selectedItem.symbol) {
      this.selectedItem = undefined;
    } else {
      this.selectedItem = r;
    }
  }
}
