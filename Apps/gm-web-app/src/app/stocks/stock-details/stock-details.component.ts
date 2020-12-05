import { Component, Input, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/app/users/authentication.service';
import { User } from 'src/app/users/user';
import { StockRecommendation } from '../stock-recommendation';
import { UserWatchlistService } from '../user-watchlist/user-watchlist.service';

@Component({
  selector: 'gm-stock-details',
  templateUrl: './stock-details.component.html',
  styleUrls: ['./stock-details.component.scss']
})
export class StockDetailsComponent implements OnInit {

  private _data: StockRecommendation | undefined;
  public fontColor!: string;
  public backgroundColor!: string;
  public isWatching: boolean;

  @Input() set data(value: StockRecommendation | undefined) {
    this._data = value;
    if (this._data) {
      this.fontColor = this._data?.getHsl(25);
    }
  }

  get data(): StockRecommendation | undefined {
    return this._data;
  }

  constructor(public watchlistService: UserWatchlistService) {
    this.isWatching = false;
    this.watchlistService.watchlistUpdate.subscribe(() => {
      if(this.data) {
        this.isWatching = this.watchlistService.includes(this.data.symbol);
      }
    })
  }

  ngOnInit(): void {
    if (this.data) {
      this.fontColor = this.data.getHsl(25);
      this.isWatching = this.watchlistService.includes(this.data.symbol);
    }
  }

  watch() {
    if(this.data) {
      this.watchlistService.addToWatchlist(this.data);
    }
  }

  unwatch() {
    if(this.data) {
      this.watchlistService.removeFromWatchlist(this.data.symbol);
    }
  }

}
