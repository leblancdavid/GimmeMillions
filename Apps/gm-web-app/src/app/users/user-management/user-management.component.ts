import { SelectionModel } from '@angular/cdk/collections';
import { unescapeIdentifier } from '@angular/compiler';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NewUserDialogComponent } from '../new-user-dialog/new-user-dialog.component';
import { User } from '../user';
import { UserService } from '../user.service';

@Component({
  selector: 'gm-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements AfterViewInit {

  displayedColumns: string[] = ['select', 'id', 'username', 'name'];
  dataSource: MatTableDataSource<User>;
  selection = new SelectionModel<User>(true, []);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private userService: UserService, public dialog: MatDialog) {
    this.dataSource = new MatTableDataSource<User>();

  }

  ngAfterViewInit() {
    this.refreshUserList();
  }

  refreshUserList() {
    this.userService.getUsers().subscribe(x => {
      this.dataSource = new MatTableDataSource<User>(x);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  deleteSelectedUsers() {
    for (const user of this.selection.selected) {
      this.userService.deleteUser(user.username).subscribe(x => {
        const index: number = this.dataSource.data.findIndex(u => u.id == user.id);
        if (index !== -1) {
          this.dataSource.data.splice(index, 1);
          this.refreshUserList();
        }
      });
    }
  }

  addNewUser() {
    const dialogRef = this.dialog.open(NewUserDialogComponent, {
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(x => {
      this.refreshUserList();
    })
  }

}
