import { Component, Input, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';
import { UserWatchlistService } from '../user-watchlist/user-watchlist.service';

@Component({
  selector: 'gm-stock-recommendation-item',
  templateUrl: './stock-recommendation-item.component.html',
  styleUrls: ['./stock-recommendation-item.component.scss']
})
export class StockRecommendationItemComponent implements OnInit {

  @Input() selected: boolean;
  public fontColor!: string;
  public backgroundColor!: string;
  private _item: StockRecommendation | undefined;
  public isWatching: boolean;

  @Input() set item(value: StockRecommendation | undefined) {
    this._item = value;
    if (this._item) {
      this.fontColor = this._item?.getHsl(25);
      this.backgroundColor = this._item.getHsl(90);
      this.isWatching = this.watchlistService.includes(this._item.symbol);
    }
  }
  get item(): StockRecommendation | undefined {
    return this._item;
  }
  constructor(public watchlistService: UserWatchlistService) { 
    this.selected = false;
    this.isWatching = false;
    this.watchlistService.watchlistUpdate.subscribe(() => {
      if(this._item) {
        this.isWatching = this.watchlistService.includes(this._item.symbol);
      }
    })
  }

  ngOnInit(): void {
    if (this._item) {
      this.fontColor = this._item.getHsl(25);
      this.backgroundColor = this._item.getHsl(90);
      this.isWatching = this.watchlistService.includes(this._item.symbol);
    }
  }

  watch() {
    if(this._item) {
      this.watchlistService.addToWatchlist(this._item);
    }
  }

  unwatch() {
    if(this._item) {
      this.watchlistService.removeFromWatchlist(this._item.symbol);
    }
  }

}
