import { Component, Input, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/app/users/authentication.service';
import { User } from 'src/app/users/user';
import { StockRecommendation } from '../stock-recommendation';

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

  constructor(public authenticationService: AuthenticationService) {
    const user = authenticationService.currentUserValue;
    this.isWatching = false;
  }

  ngOnInit(): void {
    if (this.data) {
      this.fontColor = this.data.getHsl(25);
      const user = this.authenticationService.currentUserValue;
      debugger;
      this.isWatching = user.watchlist.includes(this.data.symbol);
    }
  }

  watch() {
    if(this.data) {
      this.authenticationService.addToWatchlist(this.data.symbol).subscribe(x => {
        this.isWatching = true;
      });
    }
  }

  unwatch() {
    if(this.data) {
      this.authenticationService.addToWatchlist(this.data.symbol).subscribe(x => {
        this.isWatching = true;
      });
    }
  }

}
