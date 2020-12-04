import { Component, Input, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';

@Component({
  selector: 'gm-stock-recommendation-item',
  templateUrl: './stock-recommendation-item.component.html',
  styleUrls: ['./stock-recommendation-item.component.scss']
})
export class StockRecommendationItemComponent implements OnInit {

  @Input() item: StockRecommendation | undefined;
  
  public fontColor!: string;
  public backgroundColor!: string;

  constructor() { }

  ngOnInit(): void {
    if(this.item) {
      this.fontColor = this.item.getHsl(50);
      this.backgroundColor = this.item.getHsl(90);
    }
  }

}
