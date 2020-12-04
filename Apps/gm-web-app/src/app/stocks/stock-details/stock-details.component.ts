import { Component, Input, OnInit } from '@angular/core';
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

  @Input() set data(value: StockRecommendation | undefined) {
    this._data = value;
    if (this._data) {
      this.fontColor = this._data?.getHsl(25);
    }
  }

  get data(): StockRecommendation | undefined {
    return this._data;
  }

  constructor() { }

  ngOnInit(): void {
    if (this.data) {
      this.fontColor = this.data.getHsl(25);

    }
  }

}
