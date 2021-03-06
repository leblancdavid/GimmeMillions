import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { RecommendationHistoryDialogComponent } from '../recommendation-history-dialog/recommendation-history-dialog.component';
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
  constructor(public watchlistService: UserWatchlistService,
    public dialog: MatDialog) { 
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

  watch(event: MouseEvent) {
    event.stopImmediatePropagation();
    if(this._item) {
      this.watchlistService.addToWatchlist(this._item);
    }
  }

  unwatch(event: MouseEvent) {
    event.stopImmediatePropagation();
    if(this._item) {
      this.watchlistService.removeFromWatchlist(this._item.symbol);
    }
  }

  getDetails(event: MouseEvent) {
    event.stopImmediatePropagation();
    const dialogRef  = this.dialog.open(RecommendationHistoryDialogComponent, {
      data: { symbol: this._item?.symbol }
    })

    dialogRef.afterClosed().subscribe(result => {
      //react if needed
    });
  }

}
