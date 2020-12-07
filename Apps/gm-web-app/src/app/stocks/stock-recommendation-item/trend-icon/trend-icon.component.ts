import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'gm-trend-icon',
  templateUrl: './trend-icon.component.html',
  styleUrls: ['./trend-icon.component.scss']
})
export class TrendIconComponent implements OnInit {

  @Input() signal: number;
  @Input() color: string

  constructor() { 
    this.signal = 0.0;
    this.color = '';
  }

  ngOnInit(): void {
  }

}
