import { Component, Input, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';

@Component({
  selector: 'gm-stock-details',
  templateUrl: './stock-details.component.html',
  styleUrls: ['./stock-details.component.scss']
})
export class StockDetailsComponent implements OnInit {

  @Input() data: StockRecommendation | undefined;
  constructor() { }

  ngOnInit(): void {
  }

}
