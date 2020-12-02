import { Component, Input, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-recommendation-list',
  templateUrl: './recommendation-list.component.html',
  styleUrls: ['./recommendation-list.component.scss']
})
export class RecommendationListComponent implements OnInit {

  @Input() recommendations!: Array<StockRecommendation>;
  public selectedItem?: StockRecommendation;
  public filteredRecommendations!: Array<StockRecommendation>;
  public symbolFilter: string;
  constructor() {
    this.symbolFilter = '';
  }

  ngOnInit(): void {
    this.filteredRecommendations = this.recommendations;
  }

  select(r: StockRecommendation) {
    if (this.selectedItem && r.symbol == this.selectedItem.symbol) {
      this.selectedItem = undefined;
    } else {
      this.selectedItem = r;
    }
  }

}
